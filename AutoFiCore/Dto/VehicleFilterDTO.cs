namespace AutoFiCore.Dto
{
    public class VehicleFilterDto
    {
        public string? Make { get; set; }
        public string? Model { get; set; }
        public decimal? StartPrice { get; set; }
        public decimal? EndPrice { get; set; }
        public int? Mileage { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public string? Gearbox { get; set; }
        public string? SelectedColors { get; set; }
        public string? Status { get; set; }
    }
}
