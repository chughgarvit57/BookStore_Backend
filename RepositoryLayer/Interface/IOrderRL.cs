using RepositoryLayer.DTO;
using ModelLayer.Entity;

namespace RepositoryLayer.Interface
{
    public interface IOrderRL
    {
        Task<ResponseDTO<OrderResponseDTO>> CreateOrderAsync(CreateOrderRequestDTO request, int userId);
        Task<ResponseDTO<List<OrderedBookDTO>>> GetAllOrdersAsync(int userId);
    }
}
