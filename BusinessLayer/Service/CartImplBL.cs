using BusinessLayer.Interface;
using ModelLayer.Entity;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Service
{
    public class CartImplBL : ICartBL
    {
        private readonly ICartRL _cartRL;
        private readonly ILogger<CartImplBL> _logger;

        public CartImplBL(ICartRL cartRL, ILogger<CartImplBL> logger)
        {
            _cartRL = cartRL;
            _logger = logger;
        }

        public async Task<ResponseDTO<CartEntity>> AddInCartAsync(AddCartRequestDTO request, int userId)
        {
            try
            {
                _logger.LogInformation("Adding item to cart for userId: {UserId}", userId);
                return await _cartRL.AddInCartAsync(request, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add item to cart for userId: {UserId}", userId);
                return new ResponseDTO<CartEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<List<CartResponseDTO>>> GetUserCartAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching cart for userId: {UserId}", userId);
                return await _cartRL.GetUserCartAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch cart for userId: {UserId}", userId);
                return new ResponseDTO<List<CartResponseDTO>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<CartEntity>> UpdateCartAsync(UpdateCartRequestDTO request, int userId)
        {
            try
            {
                _logger.LogInformation("Updating cart for userId: {UserId}", userId);
                return await _cartRL.UpdateCartAsync(request, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update cart for userId: {UserId}", userId);
                return new ResponseDTO<CartEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<string>> RemoveFromCartAsync(int bookId, int userId)
        {
            try
            {
                _logger.LogInformation("Removing bookId: {BookId} from cart for userId: {UserId}", bookId, userId);
                return await _cartRL.RemoveFromCartAsync(bookId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove bookId: {BookId} from cart for userId: {UserId}", bookId, userId);
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<string>> ClearCartAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Clearing cart for userId: {UserId}", userId);
                return await _cartRL.ClearCartAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear cart for userId: {UserId}", userId);
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
