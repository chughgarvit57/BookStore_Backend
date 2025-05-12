using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace RepositoryLayer.Interface
{
    public interface IBookRL
    {
        Task<ResponseDTO<BookEntity>> AddBookAsync(AddBookRequestDTO request, int userId);
        Task<ResponseDTO<BookEntity>> GetBookAsync(int bookId);
        Task<ResponseDTO<List<BookEntity>>> GetAllBooksAsync();
    }
}
