using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface IBookBL
    {
        Task<ResponseDTO<BookEntity>> AddBookAsync(AddBookRequestDTO request, int userId);
        Task<ResponseDTO<BookEntity>> GetBookAsync(int bookId);
        Task<ResponseDTO<List<BookEntity>>> GetAllBooksAsync();
    }
}
