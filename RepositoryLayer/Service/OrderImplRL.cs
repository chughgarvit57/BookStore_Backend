using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using RepositoryLayer.Context;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;
using StackExchange.Redis;

namespace RepositoryLayer.Service
{
    public class OrderImplRL : IOrderRL
    {
        private readonly UserContext _context;
        private readonly ILogger<OrderImplRL> _logger;
        private readonly IDatabase _redisDb;
        private readonly IConnectionMultiplexer _redisConnection;

        public OrderImplRL(UserContext context, ILogger<OrderImplRL> logger, IConnectionMultiplexer redis)
        {
            _context = context;
            _logger = logger;
            _redisConnection = redis;
            _redisDb = redis.GetDatabase();
            _logger.LogDebug("OrderImplRL initialized for UserContext and Redis connection.");
        }

        private string GetUserOrderCacheKey(int userId) => $"UserOrders:{userId}";
        private string GetAllUserOrdersCacheKey() => "AllUserOrders";

        public async Task<ResponseDTO<OrderResponseDTO>> CreateOrderAsync(CreateOrderRequestDTO request, int userId)
        {
            _logger.LogInformation("Attempting to create order for UserId: {UserId}, BookId: {BookId}, AddressId: {AddressId}", userId, request.BookId, request.AddressId);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Checking book availability for BookId: {BookId}", request.BookId);
                var book = await _context.Books.FirstOrDefaultAsync(bk => bk.BookId == request.BookId);
                if (book == null || book.Quantity == 0)
                {
                    _logger.LogWarning("Book not found or out of stock for BookId: {BookId}", request.BookId);
                    return new ResponseDTO<OrderResponseDTO>
                    {
                        IsSuccess = false,
                        Message = "Book Out Of Stock!",
                    };
                }

                if (request.Quantity > book.Quantity)
                {
                    _logger.LogWarning("Requested quantity {RequestedQuantity} exceeds available quantity {AvailableQuantity} for BookId: {BookId}", request.Quantity, book.Quantity, request.BookId);
                    return new ResponseDTO<OrderResponseDTO>
                    {
                        IsSuccess = false,
                        Message = "Requested Quantity Not Available!",
                    };
                }

                if (request.Quantity <= 0)
                {
                    _logger.LogWarning("Invalid quantity {Quantity} for UserId: {UserId}, BookId: {BookId}", request.Quantity, userId, request.BookId);
                    return new ResponseDTO<OrderResponseDTO>
                    {
                        IsSuccess = false,
                        Message = "Invalid Quantity!",
                    };
                }

                _logger.LogDebug("Checking address for AddressId: {AddressId}, UserId: {UserId}", request.AddressId, userId);
                var address = await _context.Addresses.FirstOrDefaultAsync(ad => ad.AddressId == request.AddressId && ad.UserId == userId);
                if (address == null)
                {
                    _logger.LogWarning("Address not found for AddressId: {AddressId}, UserId: {UserId}", request.AddressId, userId);
                    return new ResponseDTO<OrderResponseDTO>
                    {
                        IsSuccess = false,
                        Message = "Address Not Available!",
                    };
                }

                var order = new OrderEntity
                {
                    BookId = request.BookId,
                    UserId = userId,
                    AddressId = request.AddressId,
                    OrderDate = DateTime.UtcNow,
                };
                await _context.Orders.AddAsync(order);
                _logger.LogDebug("Created new order for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);

                book.Quantity -= request.Quantity;
                _context.Books.Update(book);
                _logger.LogDebug("Updated book quantity to {NewQuantity} for BookId: {BookId}", book.Quantity, request.BookId);

                _logger.LogDebug("Checking for cart item with UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                var cartItem = await _context.Cart.FirstOrDefaultAsync(ct => ct.BookId == request.BookId && ct.UserId == userId && !ct.IsOrdered);
                if (cartItem != null)
                {
                    cartItem.IsOrdered = true;
                    cartItem.IsUncarted = true;
                    _context.Cart.Update(cartItem);
                    _logger.LogDebug("Marked cart item as ordered for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                }

                await _context.SaveChangesAsync();

                _logger.LogDebug("Clearing order caches for UserId: {UserId}", userId);
                await _redisDb.KeyDeleteAsync(GetUserOrderCacheKey(userId));
                await _redisDb.KeyDeleteAsync(GetAllUserOrdersCacheKey());

                await transaction.CommitAsync();

                _logger.LogInformation("Order placed successfully for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                return new ResponseDTO<OrderResponseDTO>
                {
                    IsSuccess = true,
                    Message = "Order Placed Successfully!",
                    Data = new OrderResponseDTO
                    {
                        BookId = request.BookId,
                        IsOrdered = true
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                await transaction.RollbackAsync();
                return new ResponseDTO<OrderResponseDTO>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<List<OrderedBookDTO>>> GetAllOrdersAsync(int userId)
        {
            _logger.LogInformation("Retrieving all orders for UserId: {UserId}", userId);
            try
            {
                var cacheKey = GetUserOrderCacheKey(userId);
                _logger.LogDebug("Checking cache for orders with key: {CacheKey}", cacheKey);
                var cachedData = await _redisDb.StringGetAsync(cacheKey);

                if (cachedData.HasValue)
                {
                    try
                    {
                        var booksFromCache = JsonSerializer.Deserialize<List<OrderedBookDTO>>(cachedData);
                        _logger.LogInformation("Retrieved {Count} ordered books from cache for UserId: {UserId}", booksFromCache?.Count ?? 0, userId);
                        return new ResponseDTO<List<OrderedBookDTO>>
                        {
                            IsSuccess = true,
                            Message = "Books Retrieved From Cache",
                            Data = booksFromCache!
                        };
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Error deserializing cached orders for UserId: {UserId}. Clearing cache.", userId);
                        await _redisDb.KeyDeleteAsync(cacheKey);
                    }
                }

                _logger.LogDebug("No cache found, querying database for orders for UserId: {UserId}", userId);
                var orders = await _context.Orders
                    .Include(o => o.Book)
                    .Where(o => o.UserId == userId)
                    .ToListAsync();

                if (orders.Count == 0)
                {
                    _logger.LogWarning("No orders found for UserId: {UserId}", userId);
                    return new ResponseDTO<List<OrderedBookDTO>>
                    {
                        IsSuccess = false,
                        Message = "No Orders Found!",
                    };
                }

                _logger.LogDebug("Mapping {Count} orders for UserId: {UserId}", orders.Count, userId);
                var orderedBooks = orders.Select(o => new OrderedBookDTO
                {
                    BookId = o.BookId,
                    BookName = o.Book!.BookName,
                    Author = o.Book!.AuthorName,
                    BookImage = o.Book!.BookImage,
                    Price = o.Book!.Price,
                    OrderedDate = o.OrderDate
                }).ToList();

                _logger.LogDebug("Caching {Count} orders for UserId: {UserId}", orderedBooks.Count, userId);
                var serializedData = JsonSerializer.Serialize(orderedBooks);
                await _redisDb.StringSetAsync(cacheKey, serializedData, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Retrieved {Count} orders from database for UserId: {UserId}", orderedBooks.Count, userId);
                return new ResponseDTO<List<OrderedBookDTO>>
                {
                    IsSuccess = true,
                    Message = "Orders Retrieved Successfully!",
                    Data = orderedBooks
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for UserId: {UserId}", userId);
                return new ResponseDTO<List<OrderedBookDTO>>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }
    }
}