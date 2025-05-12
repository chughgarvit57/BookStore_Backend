using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace RepositoryLayer.Interface
{
    public interface ICartRL
    {
        Task<ResponseDTO<CartEntity>> AddInCartAsync(AddCartRequestDTO request, int userId);
        Task<ResponseDTO<List<CartResponseDTO>>> GetUserCartAsync(int userId);
        Task<ResponseDTO<CartEntity>> UpdateCartAsync(UpdateCartRequestDTO request, int userId);
        Task<ResponseDTO<string>> RemoveFromCartAsync(int bookId, int userId);
        Task<ResponseDTO<string>> ClearCartAsync(int userId);
    }
}
