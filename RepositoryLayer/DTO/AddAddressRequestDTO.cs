using ModelLayer.Enums;

namespace RepositoryLayer.DTO
{
    public class AddAddressRequestDTO
    {
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public AddressTypes AddressType { get; set; }
        public string Locality { get; set; } = string.Empty;
        public long PhoneNumber { get; set; }
    }
}
