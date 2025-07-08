using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class Newsletter
    {
        public int Id { get; set; }

        [Required]
        [StringLength(25)]
        public string Email { get; set; } = string.Empty;
    }
}
