using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ModelLayer.Entity
{
    public class OrderEntity
    {
        [Key]
        public int OrderId { get; set; }
        [ForeignKey("Address")]
        public int AddressId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Book")]
        public int BookId { get; set; }
        public DateTime OrderDate { get; set; }
        [JsonIgnore]
        public AddressEntity? Address { get; set; }
        [JsonIgnore]
        public UserEntity? User { get; set; }
        [JsonIgnore]
        public BookEntity? Book { get; set; }
    }
}
