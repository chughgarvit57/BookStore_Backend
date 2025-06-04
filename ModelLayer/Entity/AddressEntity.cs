using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ModelLayer.Enums;

namespace ModelLayer.Entity
{
    public class AddressEntity
    {
        [Key]
        public int AddressId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public AddressTypes AddressType { get; set; }
        public string Locality { get; set; } = string.Empty;
        public long PhoneNumber { get; set; }
        [JsonIgnore]
        public UserEntity? User { get; set; }
    }
}
