using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Request
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Query { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
