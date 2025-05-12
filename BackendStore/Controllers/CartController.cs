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
    /// Controller for managing user cart operations including adding, updating, retrieving, and removing items.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartBL _cartBL;
        private readonly ILogger<CartController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CartController"/> class.
        /// </summary>
        /// <param name="cartBL">Business layer service for cart operations.</param>
        /// <param name="logger">Logger for CartController.</param>
        public CartController(ICartBL cartBL, ILogger<CartController> logger)
        {
            _cartBL = cartBL;
            _logger = logger;
            _logger.LogDebug("CartController initialized with CartBL.");
        }

        /// <summary>
        /// Adds a book to the authenticated user's cart.
        /// </summary>
        /// <param name="request">DTO containing the book ID and quantity to add to the cart.</param>
        /// <returns>
        /// Returns the added cart item details if successful, or an error message if the operation fails.
        /// </returns>
        /// <response code="200">Returns the successfully added cart item.</response>
        /// <response code="400">If the book cannot be added to the cart (e.g., invalid book ID or quantity).</response>
        [HttpPost("AddToCart")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddToCart(AddCartRequestDTO request)
        {
            _logger.LogInformation("Attempting to add item to cart for UserId: {UserId}, BookId: {BookId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value, request.BookId);
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                _logger.LogDebug("Adding to cart with details: {Request}", JsonSerializer.Serialize(request));
                var result = await _cartBL.AddInCartAsync(request, userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Item added to cart successfully for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to add item to cart for UserId: {UserId}, BookId: {BookId}. Message: {Message}", userId, request.BookId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for UserId: {UserId}, BookId: {BookId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value, request.BookId);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves all items in the authenticated user's cart.
        /// </summary>
        /// <returns>
        /// Returns a list of cart items if successful, or an error message if the operation fails.
        /// </returns>
        /// <response code="200">Returns the list of cart items.</response>
        /// <response code="400">If the cart cannot be retrieved (e.g., user not found or empty cart).</response>
        [HttpGet("GetCart")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCart()
        {
            _logger.LogInformation("Attempting to retrieve cart for UserId: {UserId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                var result = await _cartBL.GetUserCartAsync(userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Retrieved {Count} cart items for UserId: {UserId}", result.Data?.Count ?? 0, userId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve cart for UserId: {UserId}. Message: {Message}", userId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for UserId: {UserId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates the quantity of a book in the authenticated user's cart.
        /// </summary>
        /// <param name="request">DTO containing the book ID and new quantity.</param>
        /// <returns>
        /// Returns the updated cart item details if successful, or an error message if the operation fails.
        /// </returns>
        /// <response code="200">Returns the successfully updated cart item.</response>
        /// <response code="400">If the cart item cannot be updated (e.g., invalid book ID or quantity).</response>
        [HttpPatch("UpdateCart")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCart(UpdateCartRequestDTO request)
        {
            _logger.LogInformation("Attempting to update cart for UserId: {UserId}, BookId: {BookId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value, request.BookId);
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                _logger.LogDebug("Updating cart with details: {Request}", JsonSerializer.Serialize(request));
                var result = await _cartBL.UpdateCartAsync(request, userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Cart updated successfully for UserId: {UserId}, BookId: {BookId}", userId, request.BookId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to update cart for UserId: {UserId}, BookId: {BookId}. Message: {Message}", userId, request.BookId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart for UserId: {UserId}, BookId: {BookId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value, request.BookId);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Removes a book from the authenticated user's cart.
        /// </summary>
        /// <param name="bookId">The ID of the book to remove from the cart.</param>
        /// <returns>
        /// Returns a success message if the book is removed, or an error message if the operation fails.
        /// </returns>
        /// <response code="200">Returns a success message if the book was removed.</response>
        /// <response code="400">If the book cannot be removed (e.g., book not in cart).</response>
        [HttpDelete("RemoveFromCart")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            _logger.LogInformation("Attempting to remove book from cart for UserId: {UserId}, BookId: {BookId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value, bookId);
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                var result = await _cartBL.RemoveFromCartAsync(bookId, userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Book removed from cart successfully for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to remove book from cart for UserId: {UserId}, BookId: {BookId}. Message: {Message}", userId, bookId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing book from cart for UserId: {UserId}, BookId: {BookId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value, bookId);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Clears all items from the authenticated user's cart.
        /// </summary>
        /// <returns>
        /// Returns a success message if the cart is cleared, or an error message if the operation fails.
        /// </returns>
        /// <response code="200">Returns a success message if the cart was cleared.</response>
        /// <response code="400">If the cart cannot be cleared (e.g., cart is already empty).</response>
        [HttpDelete("ClearCart")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ClearCart()
        {
            _logger.LogInformation("Attempting to clear cart for UserId: {UserId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                var result = await _cartBL.ClearCartAsync(userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Cart cleared successfully for UserId: {UserId}", userId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to clear cart for UserId: {UserId}. Message: {Message}", userId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for UserId: {UserId}", User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }
    }
}