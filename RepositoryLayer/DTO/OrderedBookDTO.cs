namespace RepositoryLayer.DTO
{
    public class OrderedBookDTO
    {
        public int BookId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int OrderedQuantity { get; set; }
    }
}
