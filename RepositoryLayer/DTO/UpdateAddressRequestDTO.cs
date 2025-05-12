using ModelLayer.Enums;

namespace RepositoryLayer.DTO
{
    public class UpdateAddressRequestDTO
    {
        public int AddressId { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public AddressTypes AddressType { get; set; }
    }
}
