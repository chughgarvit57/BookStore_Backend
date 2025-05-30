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
    public class BookImplRL : IBookRL
    {
        private readonly UserContext _userContext;
        private readonly IDatabase _redisDb;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ILogger<BookImplRL> _logger;

        public BookImplRL(UserContext userContext, ILogger<BookImplRL> logger, IConnectionMultiplexer redis)
        {
            _userContext = userContext;
            _redisConnection = redis;
            _redisDb = redis.GetDatabase();
            _logger = logger;
            _logger.LogDebug("BookImplRL initialized for UserContext and Redis connection.");
        }

        private string GetBookCacheKey(int bookId) => $"Book_{bookId}";
        private string GetAllBooksCacheKey() => "all_books";

        public async Task<ResponseDTO<BookEntity>> AddBookAsync(AddBookRequestDTO request, int userId, string imageFileName)
        {
            _logger.LogInformation("Attempting to add book titled '{BookName}' for UserId: {UserId}", request.BookName, userId);
            using var transaction = await _userContext.Database.BeginTransactionAsync();
            try
            {
                _logger.LogDebug("Checking for existing book with title: {BookName}", request.BookName);
                var existingBook = await _userContext.Books.FirstOrDefaultAsync(bk => bk.BookName == request.BookName);
                if (existingBook != null)
                {
                    _logger.LogWarning("Book titled '{BookName}' already exists for UserId: {UserId}", request.BookName, userId);
                    return new ResponseDTO<BookEntity>
                    {
                        IsSuccess = false,
                        Message = $"Book titled '{request.BookName}' already exists!"
                    };
                }

                var baseUrl = "http://localhost:5057";
                var imageUrl = $"{baseUrl}/bookstore/images/{imageFileName}";
                if (string.IsNullOrEmpty(imageFileName))
                {
                    _logger.LogWarning("Image file name is empty for book titled '{BookName}'", request.BookName);
                    return new ResponseDTO<BookEntity>
                    {
                        IsSuccess = false,
                        Message = "Image file is required"
                    };
                }

                var newBook = new BookEntity
                {
                    BookName = request.BookName,
                    AuthorName = request.AuthorName,
                    Description = request.Description,
                    AuthorId = userId,
                    Price = request.Price,
                    Quantity = request.Quantity,
                    BookImage = imageUrl
                };

                _logger.LogDebug("Adding new book to database for UserId: {UserId}", userId);
                await _userContext.Books.AddAsync(newBook);
                await _userContext.SaveChangesAsync();

                _logger.LogDebug("Caching new book with BookId: {BookId}", newBook.BookId);
                var serializedBook = JsonSerializer.Serialize(newBook);
                await _redisDb.StringSetAsync(GetBookCacheKey(newBook.BookId), serializedBook, TimeSpan.FromMinutes(30));

                _logger.LogDebug("Updating all books cache");
                var allBooks = await _userContext.Books.ToListAsync();
                var serializedAllBooks = JsonSerializer.Serialize(allBooks);
                await _redisDb.StringSetAsync(GetAllBooksCacheKey(), serializedAllBooks, TimeSpan.FromMinutes(30));
                _logger.LogDebug("All books cache updated with {Count} entries", allBooks.Count);

                await transaction.CommitAsync();

                _logger.LogInformation("Book titled '{BookName}' added successfully for UserId: {UserId}", request.BookName, userId);
                return new ResponseDTO<BookEntity>
                {
                    IsSuccess = true,
                    Message = "Book added successfully",
                    Data = newBook
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding book titled '{BookName}' for UserId: {UserId}", request.BookName, userId);
                await transaction.RollbackAsync();
                return new ResponseDTO<BookEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<BookEntity>> GetBookAsync(int bookId)
        {
            _logger.LogInformation("Retrieving book with BookId: {BookId}", bookId);
            try
            {
                _logger.LogDebug("Checking cache for book with BookId: {BookId}", bookId);
                var cachedBook = await _redisDb.StringGetAsync(GetBookCacheKey(bookId));
                if (cachedBook.HasValue)
                {
                    try
                    {
                        var book = JsonSerializer.Deserialize<BookEntity>(cachedBook);
                        _logger.LogInformation("Book with BookId: {BookId} retrieved from cache", bookId);
                        return new ResponseDTO<BookEntity>
                        {
                            IsSuccess = true,
                            Message = "Book Retrieved from cache!",
                            Data = book
                        };
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Error deserializing cached book for BookId: {BookId}. Clearing cache.", bookId);
                        await _redisDb.KeyDeleteAsync(GetBookCacheKey(bookId));
                    }
                }

                _logger.LogDebug("No cache found, querying database for BookId: {BookId}", bookId);
                var bookFromDb = await _userContext.Books.FindAsync(bookId);
                if (bookFromDb == null)
                {
                    _logger.LogWarning("Book not found in database for BookId: {BookId}", bookId);
                    return new ResponseDTO<BookEntity>
                    {
                        IsSuccess = false,
                        Message = "Book Not Found"
                    };
                }

                _logger.LogDebug("Caching book with BookId: {BookId}", bookId);
                var serializedBook = JsonSerializer.Serialize(bookFromDb);
                await _redisDb.StringSetAsync(GetBookCacheKey(bookId), serializedBook, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Book with BookId: {BookId} retrieved from database", bookId);
                return new ResponseDTO<BookEntity>
                {
                    IsSuccess = true,
                    Message = "Book Retrieved from db!",
                    Data = bookFromDb
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book for BookId: {BookId}", bookId);
                return new ResponseDTO<BookEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<List<BookEntity>>> GetAllBooksAsync()
        {
            _logger.LogInformation("Retrieving all books");
            try
            {
                var cacheKey = GetAllBooksCacheKey();
                _logger.LogDebug("Checking cache for all books with key: {CacheKey}", cacheKey);
                var cachedBooks = await _redisDb.StringGetAsync(cacheKey);

                if (cachedBooks.HasValue)
                {
                    try
                    {
                        var books = JsonSerializer.Deserialize<List<BookEntity>>(cachedBooks);
                        _logger.LogInformation("Retrieved {Count} books from cache", books?.Count ?? 0);
                        return new ResponseDTO<List<BookEntity>>
                        {
                            IsSuccess = true,
                            Message = "Books retrieved from cache!",
                            Data = books
                        };
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx, "Cache corrupted or invalid format for all books. Clearing cache.");
                        await _redisDb.KeyDeleteAsync(cacheKey);
                    }
                }

                _logger.LogDebug("No cache found, querying database for all books");
                var booksFromDb = await _userContext.Books.ToListAsync();
                if (booksFromDb == null || booksFromDb.Count == 0)
                {
                    _logger.LogWarning("No books found in database");
                    return new ResponseDTO<List<BookEntity>>
                    {
                        IsSuccess = false,
                        Message = "No books found"
                    };
                }

                _logger.LogDebug("Caching {Count} books", booksFromDb.Count);
                var serializedBooks = JsonSerializer.Serialize(booksFromDb);
                await _redisDb.StringSetAsync(cacheKey, serializedBooks, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Retrieved {Count} books from database", booksFromDb.Count);
                return new ResponseDTO<List<BookEntity>>
                {
                    IsSuccess = true,
                    Message = "Books retrieved from database!",
                    Data = booksFromDb
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all books");
                return new ResponseDTO<List<BookEntity>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}