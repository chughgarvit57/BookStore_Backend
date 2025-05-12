using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface IWishListBL
    {
        Task<ResponseDTO<WishListEntity>> AddBookAsync(int bookId, int userId);
        Task<ResponseDTO<string>> RemoveBookAsync(int bookId, int userId);
        Task<ResponseDTO<List<BookEntity>>> GetAllBooksAsync(int userId);
        Task<ResponseDTO<string>> ClearWishListAsync(int userId);
    }
}
