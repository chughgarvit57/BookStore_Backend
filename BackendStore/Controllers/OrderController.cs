using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RepositoryLayer.DTO;
using System.Security.Claims;
using System.Text.Json;

namespace BackendStore.Controllers
{
    /// <summary>
    /// Controller for managing order operations including creating and retrieving orders.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderBL _orderBL;
        private readonly ILogger<OrderController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderController"/> class.
        /// </summary>
        /// <param name="orderBL">Business layer service for order operations.</param>
        /// <param name="logger">Logger for OrderController.</param>
        public OrderController(IOrderBL orderBL, ILogger<OrderController> logger)
        {
            _orderBL = orderBL;
            _logger = logger;
            _logger.LogDebug("OrderController initialized with OrderBL.");
        }

        /// <summary>
        /// Creates a new order for the authenticated user.
        /// </summary>
        /// <param name="request">DTO containing order details such as address ID and cart items.</param>
        /// <returns>
        /// Returns the created order details if successful, or an error message if the operation fails.
        /// </returns>
        /// <response code="200">Returns the successfully created order.</response>
        /// <response code="400">If the order cannot be created (e.g., invalid address or cart items).</response>
        [HttpPost("CreateOrder")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder(CreateOrderRequestDTO request)
        {
            _logger.LogInformation("Attempting to create order for UserId: {UserId}", User.FindFirst("Id")?.Value);
            try
            {
                int userId = Convert.ToInt32(User.FindFirst("Id")?.Value);
                _logger.LogDebug("Creating order with details: {Request}", JsonSerializer.Serialize(request));
                var response = await _orderBL.CreateOrderAsync(request, userId);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("Order created successfully for UserId: {UserId}", userId);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Failed to create order for UserId: {UserId}. Message: {Message}", userId, response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for UserId: {UserId}", User.FindFirst("Id")?.Value);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves all orders for the authenticated user.
        /// </summary>
        /// <returns>
        /// Returns a list of orders if successful, or an error message if the operation fails.
        /// </returns>
        /// <response code="200">Returns the list of orders.</response>
        /// <response code="400">If the orders cannot be retrieved (e.g., user not found or no orders).</response>
        [HttpGet("GetAllOrders")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllOrders()
        {
            _logger.LogInformation("Attempting to retrieve all orders for UserId: {UserId}", User.FindFirst("Id")?.Value);
            try
            {
                int userId = Convert.ToInt32(User.FindFirst("Id")?.Value);
                var response = await _orderBL.GetAllOrdersAsync(userId);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("Retrieved {Count} orders for UserId: {UserId}", response.Data?.Count ?? 0, userId);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve orders for UserId: {UserId}. Message: {Message}", userId, response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for UserId: {UserId}", User.FindFirst("Id")?.Value);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }
    }
}