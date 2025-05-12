using BusinessLayer.Interface;
using ModelLayer.Entity;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Service
{
    public class OrderImplBL : IOrderBL
    {
        private readonly IOrderRL _orderRL;
        private readonly ILogger<OrderImplBL> _logger;

        public OrderImplBL(IOrderRL orderRL, ILogger<OrderImplBL> logger)
        {
            _orderRL = orderRL;
            _logger = logger;
        }

        public async Task<ResponseDTO<OrderResponseDTO>> CreateOrderAsync(CreateOrderRequestDTO request, int userId)
        {
            try
            {
                _logger.LogInformation("Creating order for userId: {UserId}", userId);
                return await _orderRL.CreateOrderAsync(request, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order for userId: {UserId}", userId);
                return new ResponseDTO<OrderResponseDTO>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<List<OrderedBookDTO>>> GetAllOrdersAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching all orders for userId: {UserId}", userId);
                return await _orderRL.GetAllOrdersAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch orders for userId: {UserId}", userId);
                return new ResponseDTO<List<OrderedBookDTO>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
