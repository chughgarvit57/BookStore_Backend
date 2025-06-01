namespace RepositoryLayer.DTO
{
    public class CartResponseDTO
    {
        public string BookName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Image { get; set; } = string.Empty;
        public float Price { get; set; }
        public int BookId { get; set; }
        public bool IsUncarted { get; set; }
    }
}
