using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace RepositoryLayer.Interface
{
    public interface IWishListRL
    {
        Task<ResponseDTO<WishListEntity>> AddBookAsync(int bookId, int userId);
        Task<ResponseDTO<string>> RemoveBookAsync(int bookId, int userId);
        Task<ResponseDTO<List<BookEntity>>> GetAllBooksAsync(int userId);
        Task<ResponseDTO<string>> ClearWishListAsync(int userId);
    }
}
