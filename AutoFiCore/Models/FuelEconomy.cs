using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class FuelEconomy
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? FuelTankSize { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CombinedMPG { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CityMPG { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal HighwayMPG { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal CO2Emissions { get; set; }

        public Vehicle? Vehicle { get; set; } = null;
    }
}
