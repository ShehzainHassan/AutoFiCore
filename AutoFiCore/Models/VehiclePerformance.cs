using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class VehiclePerformance
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ZeroTo60MPH {  get; set; }
        public Vehicle? Vehicle { get; set; } = null;

    }
}
