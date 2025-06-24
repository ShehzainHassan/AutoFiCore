using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class Engine
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }

        [StringLength(25)]
        public string Type { get; set; } = string.Empty;

        [StringLength(4, MinimumLength = 4)]
        public string? Size { get; set; } = null;

        [Required]
        [Range(0, int.MaxValue)]
        public int Horsepower { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int? TorqueFtLBS { get; set; }

        [Range(0, int.MaxValue)]
        public int? TorqueRPM { get; set; }

        [Range(0, int.MaxValue)]
        public int? Valves { get; set; }

        [StringLength(25)]
        public string? CamType { get; set; }
        public Vehicle? Vehicle { get; set; } = null;


    }
}