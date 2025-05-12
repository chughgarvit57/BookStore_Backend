namespace RepositoryLayer.DTO
{
    public class AddBookRequestDTO
    {
        public string BookName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public float Price { get; set; }
    }
}
