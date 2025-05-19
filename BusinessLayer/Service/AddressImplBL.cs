using BusinessLayer.Interface;
using ModelLayer.Entity;
using RepositoryLayer.DTO;
using RepositoryLayer.Interface;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Service
{
    public class AddressImplBL : IAddressBL
    {
        private readonly IAddressRL _addressRL;
        private readonly ILogger<AddressImplBL> _logger;

        public AddressImplBL(IAddressRL addressRL, ILogger<AddressImplBL> logger)
        {
            _addressRL = addressRL;
            _logger = logger;
        }

        public async Task<ResponseDTO<AddressEntity>> AddAddressAsync(AddAddressRequestDTO request, int userId)
        {
            try
            {
                _logger.LogInformation("Adding address for userId: {UserId}", userId);
                return await _addressRL.AddAddressAsync(request, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding address for userId: {UserId}", userId);
                return new ResponseDTO<AddressEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<AddressEntity>> DeleteAddressAsync(int addressId, int userId)
        {
            try
            {
                _logger.LogInformation("Deleting address with addressId: {AddressId}", addressId);
                return await _addressRL.DeleteAddressAsync(addressId,userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting addressId: {AddressId}", addressId);
                return new ResponseDTO<AddressEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<List<AddressEntity>>> GetAllAddressesAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Fetching all addresses for userId: {UserId}", userId);
                return await _addressRL.GetAllAddressesAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching addresses for userId: {UserId}", userId);
                return new ResponseDTO<List<AddressEntity>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<AddressEntity>> UpdateAddressAsync(UpdateAddressRequestDTO request, int userId)
        {
            try
            {
                _logger.LogInformation("Updating address for userId: {UserId}", userId);
                return await _addressRL.UpdateAddressAsync(request, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating address for userId: {UserId}", userId);
                return new ResponseDTO<AddressEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
