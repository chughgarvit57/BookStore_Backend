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
    public class CartImplRL : ICartRL
    {
        private readonly UserContext _context;
        private readonly ILogger<CartImplRL> _logger;
        private readonly IDatabase _redisDb;
        private readonly IConnectionMultiplexer _redisConnection;

        public CartImplRL(UserContext context, ILogger<CartImplRL> logger, IConnectionMultiplexer redis)
        {
            _context = context;
            _logger = logger;
            _redisConnection = redis;
            _redisDb = redis.GetDatabase();
            _logger.LogDebug("CartImplRL initialized for UserContext and Redis connection.");
        }

        private string GetUserCartCacheKey(int userId) => $"cart:{userId}";
        private string GetCartUserCacheKey(int cartId) => $"cartuser:{cartId}";

        public async Task<ResponseDTO<CartEntity>> AddInCartAsync(AddCartRequestDTO request, int userId)
        {
            _logger.LogInformation("Attempting to add/update cart item for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Checking for existing cart item with UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                var cartItem = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == request.BookId);

                if (cartItem != null)
                {
                    if (cartItem.IsUncarted)
                    {
                        _logger.LogDebug("Reactivating uncarted item for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                        cartItem.Quantity = request.Quantity;
                        cartItem.IsUncarted = false;
                    }
                    else
                    {
                        _logger.LogDebug("Increasing quantity for existing cart item for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                        cartItem.Quantity += request.Quantity;
                    }

                    _context.Cart.Update(cartItem);
                    _logger.LogDebug("Updated existing cart item for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                }
                else
                {
                    if (request.Quantity <= 0)
                    {
                        _logger.LogWarning("Invalid quantity {Quantity} for UserId: {UserId}, BookId: {BookId}", request.Quantity, userId, request.BookId);
                        return new ResponseDTO<CartEntity>
                        {
                            IsSuccess = false,
                            Message = "Invalid quantity"
                        };
                    }

                    cartItem = new CartEntity
                    {
                        UserId = userId,
                        BookId = request.BookId,
                        Quantity = request.Quantity
                    };
                    await _context.Cart.AddAsync(cartItem);
                    _logger.LogDebug("Added new cart item for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                }

                await _context.SaveChangesAsync();

                _logger.LogDebug("Caching cart item with CartId: {CartId}", cartItem.CartId);
                var serializedCartItem = JsonSerializer.Serialize(cartItem);
                await _redisDb.KeyDeleteAsync(GetUserCartCacheKey(userId));
                await _redisDb.StringSetAsync(GetCartUserCacheKey(cartItem.CartId), serializedCartItem, TimeSpan.FromMinutes(30));

                await transaction.CommitAsync();

                _logger.LogInformation("Cart item added/updated successfully for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                return new ResponseDTO<CartEntity>
                {
                    IsSuccess = true,
                    Message = "Item added/updated in cart successfully",
                    Data = cartItem
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating cart item for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                await transaction.RollbackAsync();
                return new ResponseDTO<CartEntity>
                {
                    IsSuccess = false,
                    Message = "Failed to add item to cart"
                };
            }
        }

        public async Task<ResponseDTO<List<CartResponseDTO>>> GetUserCartAsync(int userId)
        {
            _logger.LogInformation("Retrieving cart for UserId: {UserId}", userId);
            try
            {
                var cacheKey = GetUserCartCacheKey(userId);
                _logger.LogDebug("Checking cache for cart with key: {CacheKey}", cacheKey);
                var cachedCart = await _redisDb.StringGetAsync(cacheKey);

                if (cachedCart.HasValue)
                {
                    try
                    {
                        var cachedResponse = JsonSerializer.Deserialize<ResponseDTO<List<CartResponseDTO>>>(cachedCart);
                        if (cachedResponse?.Data != null && cachedResponse.Data.Count > 0)
                        {
                            _logger.LogInformation("Retrieved {Count} cart items from cache for UserId: {UserId}", cachedResponse.Data.Count, userId);
                            return new ResponseDTO<List<CartResponseDTO>>
                            {
                                IsSuccess = true,
                                Message = "Cart retrieved from cache",
                                Data = cachedResponse.Data
                            };
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Error deserializing cached cart for UserId: {UserId}. Clearing cache.", userId);
                        await _redisDb.KeyDeleteAsync(cacheKey);
                    }
                }

                _logger.LogDebug("No valid cache found, querying database for UserId: {UserId}", userId);
                var cartItems = await _context.Cart
                    .Where(c => c.UserId == userId && !c.IsUncarted && !c.IsOrdered)
                    .Include(c => c.Book)
                    .Select(c => new CartResponseDTO
                    {
                        BookName = c.Book.BookName,
                        AuthorName = c.Book.AuthorName,
                        Description = c.Book.Description,
                        Quantity = c.Quantity
                    })
                    .ToListAsync();

                if (cartItems == null || cartItems.Count == 0)
                {
                    _logger.LogWarning("No cart items found in database for UserId: {UserId}", userId);
                    var emptyResponse = new ResponseDTO<List<CartResponseDTO>>
                    {
                        IsSuccess = false,
                        Message = "No items found in cart"
                    };

                    var serializedEmpty = JsonSerializer.Serialize(emptyResponse);
                    await _redisDb.StringSetAsync(cacheKey, serializedEmpty, TimeSpan.FromMinutes(30));
                    _logger.LogDebug("Cached empty cart response for UserId: {UserId}", userId);

                    return emptyResponse;
                }

                var response = new ResponseDTO<List<CartResponseDTO>>
                {
                    IsSuccess = true,
                    Message = "Cart retrieved successfully",
                    Data = cartItems
                };

                _logger.LogDebug("Caching {Count} cart items for UserId: {UserId}", cartItems.Count, userId);
                var serializedResponse = JsonSerializer.Serialize(response);
                await _redisDb.StringSetAsync(cacheKey, serializedResponse, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Retrieved {Count} cart items from database for UserId: {UserId}", cartItems.Count, userId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for UserId: {UserId}", userId);
                return new ResponseDTO<List<CartResponseDTO>>
                {
                    IsSuccess = false,
                    Message = "Failed to retrieve user cart"
                };
            }
        }

        public async Task<ResponseDTO<CartEntity>> UpdateCartAsync(UpdateCartRequestDTO request, int userId)
        {
            _logger.LogInformation("Attempting to update cart item for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Searching for cart item with UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                var cartItem = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == request.BookId);
                if (cartItem == null)
                {
                    _logger.LogWarning("Cart item not found for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                    return new ResponseDTO<CartEntity>
                    {
                        IsSuccess = false,
                        Message = "Cart item not found"
                    };
                }

                cartItem.Quantity = request.Quantity;
                _context.Cart.Update(cartItem);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Updated cart item quantity to {Quantity} for UserId: {UserId}, BookId: {BookId}", request.Quantity, userId, request.BookId);

                _logger.LogDebug("Caching updated cart item with CartId: {CartId}", cartItem.CartId);
                var serializedCartItem = JsonSerializer.Serialize(cartItem);
                await _redisDb.KeyDeleteAsync(GetUserCartCacheKey(userId));
                await _redisDb.StringSetAsync(GetCartUserCacheKey(cartItem.CartId), serializedCartItem, TimeSpan.FromMinutes(30));

                await transaction.CommitAsync();

                _logger.LogInformation("Cart item updated successfully for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                return new ResponseDTO<CartEntity>
                {
                    IsSuccess = true,
                    Message = "Cart item updated successfully",
                    Data = cartItem
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                await transaction.RollbackAsync();
                return new ResponseDTO<CartEntity>
                {
                    IsSuccess = false,
                    Message = "Failed to update cart item"
                };
            }
        }

        public async Task<ResponseDTO<string>> RemoveFromCartAsync(int bookId, int userId)
        {
            _logger.LogInformation("Attempting to remove cart item for UserId: {UserId}, BookId: {BookId}", userId, bookId);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Searching for cart item with UserId: {UserId}, BookId: {BookId}", userId, bookId);
                var cartItem = await _context.Cart.FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == bookId);
                if (cartItem == null)
                {
                    _logger.LogWarning("Cart item not found for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Cart item not found"
                    };
                }

                cartItem.IsUncarted = true;
                _context.Cart.Update(cartItem);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Marked cart item as uncarted for UserId: {UserId}, BookId: {BookId}", userId, bookId);

                _logger.LogDebug("Caching updated cart item with CartId: {CartId}", cartItem.CartId);
                var serializedCartItem = JsonSerializer.Serialize(cartItem);
                await _redisDb.KeyDeleteAsync(GetUserCartCacheKey(userId));
                await _redisDb.StringSetAsync(GetCartUserCacheKey(cartItem.CartId), serializedCartItem, TimeSpan.FromMinutes(30));

                await transaction.CommitAsync();

                _logger.LogInformation("Cart item removed successfully for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Cart item removed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                await transaction.RollbackAsync();
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = "Failed to remove cart item"
                };
            }
        }

        public async Task<ResponseDTO<string>> ClearCartAsync(int userId)
        {
            _logger.LogInformation("Attempting to clear cart for UserId: {UserId}", userId);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Retrieving cart items for UserId: {UserId}", userId);
                var cartItems = await _context.Cart.Where(c => c.UserId == userId).ToListAsync();
                if (cartItems.Count == 0)
                {
                    _logger.LogWarning("No cart items found for UserId: {UserId}", userId);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "No items found in cart"
                    };
                }

                _logger.LogDebug("Marking {Count} cart items as uncarted for UserId: {UserId}", cartItems.Count, userId);
                foreach (var item in cartItems)
                {
                    item.IsUncarted = true;
                    _context.Cart.Update(item);
                }
                await _context.SaveChangesAsync();

                _logger.LogDebug("Clearing user cart cache for UserId: {UserId}", userId);
                await _redisDb.KeyDeleteAsync(GetUserCartCacheKey(userId));

                await transaction.CommitAsync();

                _logger.LogInformation("Cart cleared successfully for UserId: {UserId}", userId);
                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Cart cleared successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for UserId: {UserId}", userId);
                await transaction.RollbackAsync();
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = "Failed to clear cart"
                };
            }
        }
    }
}