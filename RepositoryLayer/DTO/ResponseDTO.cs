namespace RepositoryLayer.DTO
{
    public class ResponseDTO<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; }
    }
}
