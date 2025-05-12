using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ModelLayer.Entity
{
    public class BookEntity
    {
        [Key]
        public int BookId { get; set; }
        [ForeignKey("User")]
        public int AuthorId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public string BookImage { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public float Price { get; set; }
        [JsonIgnore]
        public UserEntity? User { get; set; }
    }
}
