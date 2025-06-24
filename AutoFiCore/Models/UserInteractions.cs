using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoFiCore.Models
{
    public class UserInteractions
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(50)]
        public string InteractionType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; } = null;

        public Vehicle? Vehicle { get; set; } = null;
    }
}
