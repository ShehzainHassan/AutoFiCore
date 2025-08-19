namespace AutoFiCore.Dto
{
    /// <summary>
    /// Represents a set of optional filters used to query vehicles.
    /// </summary>
    public class VehicleFilterDto
    {
        /// <summary>
        /// The make of the vehicle (e.g., Toyota, Honda).
        /// </summary>
        public string? Make { get; set; }

        /// <summary>
        /// The model of the vehicle (e.g., Corolla, Civic).
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// The minimum price of the vehicle.
        /// </summary>
        public decimal? StartPrice { get; set; }

        /// <summary>
        /// The maximum price of the vehicle.
        /// </summary>
        public decimal? EndPrice { get; set; }

        /// <summary>
        /// The maximum mileage of the vehicle.
        /// </summary>
        public int? Mileage { get; set; }

        /// <summary>
        /// The earliest manufacturing year of the vehicle.
        /// </summary>
        public int? StartYear { get; set; }

        /// <summary>
        /// The latest manufacturing year of the vehicle.
        /// </summary>
        public int? EndYear { get; set; }

        /// <summary>
        /// The type of gearbox (e.g., Automatic, Manual).
        /// </summary>
        public string? Gearbox { get; set; }

        /// <summary>
        /// The selected color(s) of the vehicle.
        /// </summary>
        public string? SelectedColors { get; set; }

        /// <summary>
        /// The status of the vehicle (e.g., USED, NEW).
        /// </summary>
        public string? Status { get; set; }
    }
}