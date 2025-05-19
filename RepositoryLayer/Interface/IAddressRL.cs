using ModelLayer.Entity;
using RepositoryLayer.DTO;

namespace RepositoryLayer.Interface
{
    public interface IAddressRL
    {
        Task<ResponseDTO<AddressEntity>> AddAddressAsync(AddAddressRequestDTO request, int userId);
        Task<ResponseDTO<AddressEntity>> DeleteAddressAsync(int addressId, int userId);
        Task<ResponseDTO<List<AddressEntity>>> GetAllAddressesAsync(int userId);
        Task<ResponseDTO<AddressEntity>> UpdateAddressAsync(UpdateAddressRequestDTO request, int userId);
    }
}
