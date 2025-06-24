namespace AutoFiCore.Models
{
    public class VehicleListResult
    {
        public IEnumerable<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public int TotalCount { get; set; }

        public Dictionary<string, int> GearboxCounts { get; set; } = new();
        public Dictionary<string, int> ColorCounts { get; set; } = new();

    }

}
