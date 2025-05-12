using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface IOrderBL
    {
        Task<ResponseDTO<OrderResponseDTO>> CreateOrderAsync(CreateOrderRequestDTO request, int userId);
        Task<ResponseDTO<List<OrderedBookDTO>>> GetAllOrdersAsync(int userId);
    }
}
