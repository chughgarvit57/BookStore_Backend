using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ModelLayer.Entity
{
    public class CartEntity
    {
        [Key]
        public int CartId { get; set; }
        public int Quantity { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Book")]
        public int BookId { get; set; }
        public bool IsOrdered { get; set; } = false;
        public bool IsUncarted { get; set; } = false;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [JsonIgnore]
        public UserEntity? User { get; set; }
        [JsonIgnore]
        public BookEntity? Book { get; set; }
    }
}
