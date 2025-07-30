using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Request")]
        public int RequestId { get; set; }
        public Request Request { get; set; } // Связь с запросом

        [Required]
        [StringLength(50)]
        public string Marketplace { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Brand { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public string Image { get; set; }

        [Required]
        [Column(TypeName = "numeric(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "numeric(3,2)")]
        public decimal Rating { get; set; } = 0;

        public int Reviews { get; set; } = 0;

        public ICollection<PriceHistory> PriceHistories { get; set; }
    }
}
