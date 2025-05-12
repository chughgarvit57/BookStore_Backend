using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ModelLayer.Entity
{
    public class WishListEntity
    {
        [Key]
        public int WishListId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Book")]
        public int BookId { get; set; }
        [JsonIgnore]
        public UserEntity? User { get; set; }
        [JsonIgnore]
        public BookEntity? Book { get; set; }
    }
}
