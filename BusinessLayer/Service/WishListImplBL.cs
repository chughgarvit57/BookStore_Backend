using BusinessLayer.Interface;
using ModelLayer.Entity;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Service
{
    public class WishListImplBL : IWishListBL
    {
        private readonly IWishListRL _wishListRL;
        private readonly ILogger<WishListImplBL> _logger;

        public WishListImplBL(IWishListRL wishListRL, ILogger<WishListImplBL> logger)
        {
            _wishListRL = wishListRL;
            _logger = logger;
        }

        public async Task<ResponseDTO<WishListEntity>> AddBookAsync(int bookId, int userId)
        {
            try
            {
                _logger.LogInformation("Adding bookId: {BookId} to userId: {UserId}'s wishlist", bookId, userId);
                return await _wishListRL.AddBookAsync(bookId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add bookId: {BookId} to userId: {UserId}'s wishlist", bookId, userId);
                return new ResponseDTO<WishListEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<string>> RemoveBookAsync(int bookId, int userId)
        {
            try
            {
                _logger.LogInformation("Removing bookId: {BookId} from userId: {UserId}'s wishlist", bookId, userId);
                return await _wishListRL.RemoveBookAsync(bookId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove bookId: {BookId} from userId: {UserId}'s wishlist", bookId, userId);
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<List<BookEntity>>> GetAllBooksAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching wishlist books for userId: {UserId}", userId);
                return await _wishListRL.GetAllBooksAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch wishlist books for userId: {UserId}", userId);
                return new ResponseDTO<List<BookEntity>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<string>> ClearWishListAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Clearing wishlist for userId: {UserId}", userId);
                return await _wishListRL.ClearWishListAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear wishlist for userId: {UserId}", userId);
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
