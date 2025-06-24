using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(25)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

    }
}
