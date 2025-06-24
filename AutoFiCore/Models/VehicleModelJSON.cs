namespace AutoFiCore.Models
{
    public class VehicleModelJSON
    {
            public string? Make { get; set; }
            public string? Model { get; set; }
            public int Year { get; set; }
            public VehicleFeaturesJSON? Features { get; set; }
    }
}
