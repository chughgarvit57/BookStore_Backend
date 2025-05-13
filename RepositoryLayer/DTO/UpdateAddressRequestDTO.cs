using ModelLayer.Enums;
using System.ComponentModel.DataAnnotations;

namespace RepositoryLayer.DTO
{
    public class UpdateAddressRequestDTO
    {
        [Required(ErrorMessage = "Address ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Address ID must be a positive number.")]
        public int AddressId { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address type is required.")]
        public AddressTypes AddressType { get; set; }
    }
}