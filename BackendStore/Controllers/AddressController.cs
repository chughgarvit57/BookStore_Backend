using System.Text.Json;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RepositoryLayer.DTO;

namespace BackendStore.Controllers
{
    /// <summary>
    /// Controller for managing user addresses
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressBL _addressBL;
        private readonly ILogger<AddressController> _logger;

        /// <summary>
        /// Initializes a new instance of the AddressController class
        /// </summary>
        /// <param name="addressBL">Business layer service for address operations</param>
        /// <param name="logger">Logger for AddressController</param>
        public AddressController(IAddressBL addressBL, ILogger<AddressController> logger)
        {
            _addressBL = addressBL;
            _logger = logger;
            _logger.LogDebug("AddressController initialized with AddressBL.");
        }

        /// <summary>
        /// Adds a new address for the authenticated user
        /// </summary>
        /// <param name="request">Address details to add</param>
        /// <returns>Response with the added address or error message</returns>
        /// <response code="200">Returns the successfully added address</response>
        /// <response code="400">Returns if the address couldn't be added</response>
        [HttpPost("AddAddress")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAddress(AddAddressRequestDTO request)
        {
            _logger.LogInformation("Attempting to add address for UserId: {UserId}", User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                _logger.LogDebug("Processing, Adding address with details: {Request}", JsonSerializer.Serialize(request));
                var response = await _addressBL.AddAddressAsync(request, userId);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("Address added successfully for UserId: {UserId}", userId);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Failed to add address for UserId: {UserId}. Message: {Message}", userId, response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address for UserId: {UserId}", User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes an address by its ID
        /// </summary>
        /// <param name="addressId">ID of the address to delete</param>
        /// <returns>Response indicating success or failure</returns>
        /// <response code="200">Returns success message if address was deleted</response>
        /// <response code="400">Returns if address couldn't be deleted</response>
        [HttpDelete("AddressId")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAddressAsync(int addressId)
        {
            _logger.LogInformation("Attempting to delete address with AddressId: {AddressId}", addressId);
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                var result = await _addressBL.DeleteAddressAsync(addressId, userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Address deleted successfully with AddressId: {AddressId}", addressId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to delete address with AddressId: {AddressId}. Message: {Message}", addressId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address with AddressId: {AddressId}", addressId);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets all addresses for the authenticated user
        /// </summary>
        /// <returns>List of user addresses</returns>
        /// <response code="200">Returns the list of addresses</response>
        /// <response code="400">Returns if addresses couldn't be retrieved</response>
        [HttpGet("GetAllAddresses")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAddresses()
        {
            _logger.LogInformation("Attempting to retrieve all addresses for UserId: {UserId}", User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var result = await _addressBL.GetAllAddressesAsync(userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Retrieved {Count} addresses for UserId: {UserId}", result.Data?.Count ?? 0, userId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve addresses for UserId: {UserId}. Message: {Message}", userId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses for UserId: {UserId}", User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates an existing address for the authenticated user
        /// </summary>
        /// <param name="request">Updated address details</param>
        /// <returns>Response with the updated address or error message</returns>
        /// <response code="200">Returns the successfully updated address</response>
        /// <response code="400">Returns if the address couldn't be updated</response>
        [HttpPut("UpdateAddress")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAddress(UpdateAddressRequestDTO request)
        {
            _logger.LogInformation("Attempting to update address for UserId: {UserId}, AddressId: {AddressId}", User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value, request.AddressId);
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                _logger.LogDebug("Updating address with details: {Request}", JsonSerializer.Serialize(request));
                var result = await _addressBL.UpdateAddressAsync(request, userId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Address updated successfully for UserId: {UserId}, AddressId: {AddressId}", userId, request.AddressId);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Failed to update address for UserId: {UserId}, AddressId: {AddressId}. Message: {Message}", userId, request.AddressId, result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address for UserId: {UserId}, AddressId: {AddressId}", User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value, request.AddressId);
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }
    }
}