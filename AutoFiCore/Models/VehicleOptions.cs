using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class VehicleOptions
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public List<string> Options { get; set; } = new();
        public List<Vehicle>? Vehicle { get; set; } = null;
    }
}
