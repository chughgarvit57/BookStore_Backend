namespace RepositoryLayer.DTO
{
    public class CreateOrderRequestDTO
    {
        public int BookId { get; set; }
        public int AddressId { get; set; }
        public int Quantity { get; set; }
    }
}
    