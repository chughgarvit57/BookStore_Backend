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
    public class WishListImplRL : IWishListRL
    {
        private readonly UserContext _context;
        private readonly ILogger<WishListImplRL> _logger;
        private readonly IDatabase _redisDb;
        private readonly IBookRL _bookRL;

        public WishListImplRL(UserContext context, ILogger<WishListImplRL> logger, IConnectionMultiplexer redis, IBookRL bookRL)
        {
            _context = context;
            _logger = logger;
            _redisDb = redis.GetDatabase();
            _bookRL = bookRL;
            _logger.LogDebug("WishListImplRL initialized for UserContext, Redis, and BookRL.");
        }

        private string GetWishListBooksKey(int userId) => $"User:{userId}:WishList:Books";
        private string GetWishListCountKey(int userId) => $"User:{userId}:WishList:Count";

        public async Task<ResponseDTO<WishListEntity>> AddBookAsync(int bookId, int userId)
        {
            _logger.LogInformation("Attempting to add book to wishlist for UserId: {UserId}, BookId: {BookId}", userId, bookId);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Checking if book exists in wishlist for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                var existingWishList = await _context.WishList.FirstOrDefaultAsync(w => w.UserId == userId && w.BookId == bookId);
                if (existingWishList != null)
                {
                    _logger.LogWarning("Book already exists in wishlist for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                    return new ResponseDTO<WishListEntity>
                    {
                        IsSuccess = false,
                        Message = "Book already exists in the wishlist."
                    };
                }

                var wishList = new WishListEntity { UserId = userId, BookId = bookId };
                _logger.LogDebug("Adding new wishlist entry for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                await _context.WishList.AddAsync(wishList);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Fetching updated wishlist books for UserId: {UserId}", userId);
                var wishlistBooks = await _context.WishList
                    .Where(w => w.UserId == userId)
                    .Include(w => w.Book)
                    .Select(w => w.Book)
                    .ToListAsync();

                _logger.LogDebug("Caching {Count} wishlist books for UserId: {UserId}", wishlistBooks.Count, userId);
                var serialized = JsonSerializer.Serialize(wishlistBooks);
                await _redisDb.StringSetAsync(GetWishListBooksKey(userId), serialized, TimeSpan.FromMinutes(30));
                await _redisDb.StringSetAsync(GetWishListCountKey(userId), wishlistBooks.Count, TimeSpan.FromMinutes(30));

                await transaction.CommitAsync();

                _logger.LogInformation("Book added to wishlist successfully for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                return new ResponseDTO<WishListEntity>
                {
                    IsSuccess = true,
                    Message = "Book added to wishlist successfully.",
                    Data = wishList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding book to wishlist for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                await transaction.RollbackAsync();
                return new ResponseDTO<WishListEntity>
                {
                    IsSuccess = false,
                    Message = "Error adding book to wishlist."
                };
            }
        }

        public async Task<ResponseDTO<string>> RemoveBookAsync(int bookId, int userId)
        {
            _logger.LogInformation("Attempting to remove book from wishlist for UserId: {UserId}, BookId: {BookId}", userId, bookId);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Checking if book exists in wishlist for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                var existingWishList = await _context.WishList.FirstOrDefaultAsync(w => w.UserId == userId && w.BookId == bookId);
                if (existingWishList == null)
                {
                    _logger.LogWarning("Book not found in wishlist for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Book not found in the wishlist."
                    };
                }

                _logger.LogDebug("Removing wishlist entry for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                _context.WishList.Remove(existingWishList);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Fetching remaining wishlist books for UserId: {UserId}", userId);
                var remainingBooks = await _context.WishList
                    .Where(w => w.UserId == userId)
                    .Include(w => w.Book)
                    .Select(w => w.Book)
                    .ToListAsync();

                _logger.LogDebug("Caching {Count} remaining wishlist books for UserId: {UserId}", remainingBooks.Count, userId);
                var serialized = JsonSerializer.Serialize(remainingBooks);
                await _redisDb.StringSetAsync(GetWishListBooksKey(userId), serialized, TimeSpan.FromMinutes(30));
                await _redisDb.StringSetAsync(GetWishListCountKey(userId), remainingBooks.Count, TimeSpan.FromMinutes(30));

                await transaction.CommitAsync();

                _logger.LogInformation("Book removed from wishlist successfully for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Book removed from wishlist successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing book from wishlist for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                await transaction.RollbackAsync();
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = "Error removing book from wishlist."
                };
            }
        }

        public async Task<ResponseDTO<List<BookEntity>>> GetAllBooksAsync(int userId)
        {
            _logger.LogInformation("Retrieving wishlist books for UserId: {UserId}", userId);
            try
            {
                string redisKey = GetWishListBooksKey(userId);
                _logger.LogDebug("Checking cache for wishlist books with key: {RedisKey}", redisKey);
                var cachedBooks = await _redisDb.StringGetAsync(redisKey);

                if (cachedBooks.HasValue)
                {
                    try
                    {
                        var booksFromCache = JsonSerializer.Deserialize<List<BookEntity>>(cachedBooks);
                        _logger.LogInformation("Retrieved {Count} wishlist books from cache for UserId: {UserId}", booksFromCache?.Count ?? 0, userId);
                        return new ResponseDTO<List<BookEntity>>
                        {
                            IsSuccess = true,
                            Data = booksFromCache,
                            Message = booksFromCache != null && booksFromCache.Any()
                                ? "Wishlist books retrieved from cache."
                                : "No books found in wishlist."
                        };
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Error deserializing cached wishlist books for UserId: {UserId}. Clearing cache.", userId);
                        await _redisDb.KeyDeleteAsync(redisKey);
                    }
                }

                _logger.LogDebug("No cache found, querying database for wishlist books for UserId: {UserId}", userId);
                var wishlistBooks = await _context.WishList
                    .Where(w => w.UserId == userId)
                    .Include(w => w.Book)
                    .Select(w => w.Book)
                    .ToListAsync();

                _logger.LogDebug("Caching {Count} wishlist books for UserId: {UserId}", wishlistBooks.Count, userId);
                var serialized = JsonSerializer.Serialize(wishlistBooks);
                await _redisDb.StringSetAsync(redisKey, serialized, TimeSpan.FromMinutes(30));
                await _redisDb.StringSetAsync(GetWishListCountKey(userId), wishlistBooks.Count, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Retrieved {Count} wishlist books from database for UserId: {UserId}", wishlistBooks.Count, userId);
                return new ResponseDTO<List<BookEntity>>
                {
                    IsSuccess = true,
                    Data = wishlistBooks,
                    Message = wishlistBooks.Any()
                        ? "Wishlist books retrieved from database."
                        : "No books found in wishlist."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wishlist books for UserId: {UserId}", userId);
                return new ResponseDTO<List<BookEntity>>
                {
                    IsSuccess = false,
                    Message = "Error retrieving wishlist books."
                };
            }
        }

        public async Task<ResponseDTO<string>> ClearWishListAsync(int userId)
        {
            _logger.LogInformation("Attempting to clear wishlist for UserId: {UserId}", userId);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Checking for wishlist entries for UserId: {UserId}", userId);
                var existingWishList = await _context.WishList.Where(w => w.UserId == userId).ToListAsync();
                if (!existingWishList.Any())
                {
                    _logger.LogWarning("No books found in wishlist for UserId: {UserId}", userId);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "No books found in the wishlist."
                    };
                }

                _logger.LogDebug("Removing {Count} wishlist entries for UserId: {UserId}", existingWishList.Count, userId);
                _context.WishList.RemoveRange(existingWishList);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Clearing wishlist caches for UserId: {UserId}", userId);
                await _redisDb.KeyDeleteAsync(GetWishListBooksKey(userId));
                await _redisDb.KeyDeleteAsync(GetWishListCountKey(userId));

                await transaction.CommitAsync();

                _logger.LogInformation("Wishlist cleared successfully for UserId: {UserId}", userId);
                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Wishlist cleared successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing wishlist for UserId: {UserId}", userId);
                await transaction.RollbackAsync();
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = "Error clearing wishlist."
                };
            }
        }
    }
}