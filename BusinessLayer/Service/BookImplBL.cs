using BusinessLayer.Interface;
using ModelLayer.Entity;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Service
{
    public class BookImplBL : IBookBL
    {
        private readonly IBookRL _bookRL;
        private readonly ILogger<BookImplBL> _logger;

        public BookImplBL(IBookRL bookRL, ILogger<BookImplBL> logger)
        {
            _bookRL = bookRL;
            _logger = logger;
        }

        public async Task<ResponseDTO<BookEntity>> AddBookAsync(AddBookRequestDTO request, int userId, string imageFileName)
        {
            try
            {
                _logger.LogInformation("Adding new book for userId: {UserId}", userId);
                return await _bookRL.AddBookAsync(request, userId, imageFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding book for userId: {UserId}", userId);
                return new ResponseDTO<BookEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<BookEntity>> GetBookAsync(int bookId)
        {
            try
            {
                _logger.LogInformation("Fetching book with bookId: {BookId}", bookId);
                return await _bookRL.GetBookAsync(bookId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching bookId: {BookId}", bookId);
                return new ResponseDTO<BookEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<List<BookEntity>>> GetAllBooksAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all books");
                return await _bookRL.GetAllBooksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all books");
                return new ResponseDTO<List<BookEntity>>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }
    }
}
