using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RepositoryLayer.Interface;
using RepositoryLayer.DTO;

namespace BackendStore.Controllers
{
    /// <summary>
    /// Controller for managing user wishlist operations including adding, removing, retrieving, and clearing books.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WishListController : ControllerBase
    {
        private readonly IWishListRL _wishlistBL;
        private readonly ILogger<WishListController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WishListController"/> class.
        /// </summary>
        /// <param name="wishlistBL">Repository layer service for wishlist operations.</param>
        /// <param name="logger">Logger for WishListController.</param>
        public WishListController(IWishListRL wishlistBL, ILogger<WishListController> logger)
        {
            _wishlistBL = wishlistBL;
            _logger = logger;
            _logger.LogDebug("WishListController initialized with WishListRL.");
        }

        /// <summary>
        /// Adds a book to the authenticated user's wishlist.
        /// </summary>
        /// <param name="bookId">The ID of the book to add to the wishlist.</param>
        /// <returns>
        /// Returns the added wishlist item details if successful, or an error message if the operation fails.
        /// </returns>
        /// <response code="200">Returns the successfully added wishlist item.</response>
        /// <response code="400">If the book cannot be added to the wishlist (e.g., already exists or invalid book ID).</response>
        [HttpPost("AddBookToWishList")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddToWishList(int bookId)
        {
            _logger.LogInformation("Attempting to add book to wishlist for UserId: {UserId}, BookId: {BookId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value, bookId);
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                var result = await _wishlistBL.AddBookAsync(bookId, userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Book added to wishlist successfully for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to add book to wishlist for UserId: {UserId}, BookId: {BookId}. Message: {Message}", userId, bookId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding book to wishlist for UserId: {UserId}, BookId: {BookId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value, bookId);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Removes a book from the authenticated user's wishlist.
        /// </summary>
        /// <param name="bookId">The ID of the book to remove from the wishlist.</param>
        /// <returns>
        /// Returns a success message if the book is removed, or an error message if the operation fails.
        /// </returns>
        /// <response code="200">Returns a success message if the book was removed.</response>
        /// <response code="400">If the book cannot be removed (e.g., not in wishlist).</response>
        [HttpDelete("RemoveBookFromWishList")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveFromWishList(int bookId)
        {
            _logger.LogInformation("Attempting to remove book from wishlist for UserId: {UserId}, BookId: {BookId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value, bookId);
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                var result = await _wishlistBL.RemoveBookAsync(bookId, userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Book removed from wishlist successfully for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to remove book from wishlist for UserId: {UserId}, BookId: {BookId}. Message: {Message}", userId, bookId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing book from wishlist for UserId: {UserId}, BookId: {BookId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value, bookId);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves all books in the authenticated user's wishlist.
        /// </summary>
        /// <returns>
        /// Returns a list of books in the wishlist if successful, or an error message if the operation fails.
        /// </returns>
        /// <response code="200">Returns the list of books in the wishlist.</response>
        /// <response code="400">If the wishlist cannot be retrieved (e.g., empty or user not found).</response>
        [HttpGet("GetAllBooksInWishList")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllBooksInWishList()
        {
            _logger.LogInformation("Attempting to retrieve wishlist for UserId: {UserId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                var result = await _wishlistBL.GetAllBooksAsync(userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Retrieved {Count} wishlist books for UserId: {UserId}", result.Data?.Count ?? 0, userId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve wishlist for UserId: {UserId}. Message: {Message}", userId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wishlist for UserId: {UserId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Clears all books from the authenticated user's wishlist.
        /// </summary>
        /// <returns>
        /// Returns a success message if the wishlist is cleared, or an error message if the operation fails.
        /// </returns>
        /// <response code="200">Returns a success message if the wishlist was cleared.</response>
        /// <response code="400">If the wishlist cannot be cleared (e.g., already empty).</response>
        [HttpDelete("ClearWishList")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ClearWishList()
        {
            _logger.LogInformation("Attempting to clear wishlist for UserId: {UserId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                var result = await _wishlistBL.ClearWishListAsync(userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Wishlist cleared successfully for UserId: {UserId}", userId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to clear wishlist for UserId: {UserId}. Message: {Message}", userId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing wishlist for UserId: {UserId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }
    }
}