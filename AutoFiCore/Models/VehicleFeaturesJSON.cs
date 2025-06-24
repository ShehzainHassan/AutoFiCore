namespace AutoFiCore.Models
{
    public class VehicleFeaturesJSON
    {
        public Drivetrain? Drivetrain { get; set; }
        public Engine? Engine { get; set; }
        public FuelEconomy? FuelEconomy { get; set; }
        public VehiclePerformance? Performance { get; set; }
        public Measurements? Measurements { get; set; }
        public List<string>? Options { get; set; }
    }
}
