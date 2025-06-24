using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class Drivetrain
    {

        public int Id { get; set; }
        public int VehicleId { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Transmission { get; set; } = string.Empty;

        public Vehicle? Vehicle { get; set; } = null;

    }
}
