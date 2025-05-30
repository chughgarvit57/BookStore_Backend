using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface IBookBL
    {
        Task<ResponseDTO<BookEntity>> AddBookAsync(AddBookRequestDTO request, int userId, string imageFIleName);
        Task<ResponseDTO<BookEntity>> GetBookAsync(int bookId);
        Task<ResponseDTO<List<BookEntity>>> GetAllBooksAsync();
    }
}
