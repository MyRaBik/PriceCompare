using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required] //Проверить
        [StringLength(50)]
        public string Email { get; set; } = default!;

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!; 

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "user"; // Роли: user, admin
    }
}
