namespace AutoFiCore.Dto
{
    public class AuctionAnalyticsTableDTO
    {
        public int AuctionId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string VehicleCategory { get; set; } = string.Empty;
        public int Views { get; set; }
        public int Bidders { get; set; }
        public int Bids { get; set; }
        public decimal FinalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
