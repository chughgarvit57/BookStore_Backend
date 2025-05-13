using System.ComponentModel.DataAnnotations;

namespace RepositoryLayer.DTO
{
    public class CreateOrderRequestDTO
    {
        [Required(ErrorMessage = "Book ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Book ID must be a positive number.")]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Address ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Address ID must be a positive number.")]
        public int AddressId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}