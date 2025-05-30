using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RepositoryLayer.DTO
{
    public class AddBookRequestDTO
    {
        [Required(ErrorMessage = "Book name is required.")]
        [StringLength(100, ErrorMessage = "Book name cannot exceed 100 characters.")]
        public string BookName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author name is required.")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
        public string AuthorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, float.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public float Price { get; set; }
        [Required(ErrorMessage = "Image file is required.")]
        public IFormFile? BookImage { get; set; }
    }
}