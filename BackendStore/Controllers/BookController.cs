using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RepositoryLayer.DTO;
using System.Text.Json;

namespace BackendStore.Controllers
{
    /// <summary>
    /// Controller for handling book-related operations including adding, retrieving, and managing books.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookBL _bookBL;
        private readonly ILogger<BookController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookController"/> class.
        /// </summary>
        /// <param name="bookBL">Business layer service for book operations.</param>
        /// <param name="logger">Logger for BookController.</param>
        public BookController(IBookBL bookBL, ILogger<BookController> logger)
        {
            _bookBL = bookBL;
            _logger = logger;
            _logger.LogDebug("BookController initialized with BookBL.");
        }

        /// <summary>
        /// Adds a new book to the system.
        /// </summary>
        /// <param name="request">DTO containing book details including name, author, description, price, quantity, and image.</param>
        /// <returns>
        /// Returns the created book details if successful, or error details if the operation fails.
        /// </returns>
        /// <response code="200">Returns the newly created book.</response>
        /// <response code="400">If the book data is invalid or creation fails.</response>
        [HttpPost("AddBook")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<IActionResult> AddBook(AddBookRequestDTO request)
        {
            _logger.LogInformation("Attempting to add book for UserId: {UserId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                _logger.LogDebug("Adding book with details: {Request}", JsonSerializer.Serialize(request));
                var result = await _bookBL.AddBookAsync(request, userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Book added successfully for UserId: {UserId}", userId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to add book for UserId: {UserId}. Message: {Message}", userId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding book for UserId: {UserId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves the details of a specific book by its ID.
        /// </summary>
        /// <param name="bookId">The unique identifier of the book to retrieve.</param>
        /// <returns>
        /// Returns the book details if found; otherwise, returns an error message.
        /// </returns>
        /// <response code="200">Returns the details of the requested book.</response>
        /// <response code="400">If the bookId is invalid or the book is not found.</response>
        [HttpGet("BookId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<IActionResult> GetBook(int bookId)
        {
            _logger.LogInformation("Attempting to retrieve book with BookId: {BookId}", bookId);
            try
            {
                var result = await _bookBL.GetBookAsync(bookId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Book retrieved successfully with BookId: {BookId}", bookId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve book with BookId: {BookId}. Message: {Message}", bookId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book with BookId: {BookId}", bookId);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves a list of all books available in the system.
        /// </summary>
        /// <returns>
        /// Returns a list of all books if successful; otherwise, returns an error message.
        /// </returns>
        /// <response code="200">Returns the list of books.</response>
        /// <response code="400">If retrieval fails or an error occurs.</response>
        [HttpGet("GetAllBooks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<IActionResult> GetAllBooks()
        {
            _logger.LogInformation("Attempting to retrieve all books");
            try
            {
                var result = await _bookBL.GetAllBooksAsync();
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Retrieved {Count} books successfully", result.Data?.Count ?? 0);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve all books. Message: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all books");
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }
    }
}