using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class PriceHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        [JsonIgnore]
        public Product Product { get; set; }
    }
}
