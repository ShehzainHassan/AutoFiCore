namespace AutoFiCore.Dto
{
    public class VehicleDTO
    {
        public int Id { get; set; }
        public string Vin { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Price { get; set; }
        public int Mileage { get; set; }
        public string Color { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public string Transmission { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

}
