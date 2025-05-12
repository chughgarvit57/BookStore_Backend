using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface IAddressBL
    {
        Task<ResponseDTO<AddressEntity>> AddAddressAsync(AddAddressRequestDTO request, int userId);
        Task<ResponseDTO<AddressEntity>> DeleteAddressAsync(int addressId);
        Task<ResponseDTO<List<AddressEntity>>> GetAllAddressesAsync(int userId);
        Task<ResponseDTO<AddressEntity>> UpdateAddressAsync(UpdateAddressRequestDTO request, int userId);
    }
}
