using ModelLayer.Enums;
using System.ComponentModel.DataAnnotations;

namespace RepositoryLayer.DTO
{
    public class AddAddressRequestDTO
    {
        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address type is required.")]
        public AddressTypes AddressType { get; set; }

        [StringLength(100, ErrorMessage = "Locality cannot exceed 100 characters.")]
        public string Locality { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Range(1000000000, 9999999999, ErrorMessage = "Phone number must be exactly 10 digits.")]
        public long PhoneNumber { get; set; }
    }
}