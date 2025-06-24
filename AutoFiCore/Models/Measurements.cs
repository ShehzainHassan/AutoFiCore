using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class Measurements
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }

        [Required]
        [Range(2, int.MaxValue)]
        public int Doors { get; set; }

        [Required]
        [Range(2, int.MaxValue)]
        public int MaximumSeating { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal HeightInches { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal WidthInches { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal LengthInches { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal WheelbaseInches { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal GroundClearance { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? CargoCapacityCuFt { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CurbWeightLBS { get; set; }
        public Vehicle? Vehicle { get; set; } = null;

    }
}
