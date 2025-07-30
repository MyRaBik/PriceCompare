namespace Domain.DTOs
{
    public class RequestDto
    {
        public int Id { get; set; }
        public string Query { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
