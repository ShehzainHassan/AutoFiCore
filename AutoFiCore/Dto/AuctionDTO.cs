namespace AutoFiCore.Dto
{
    public class AuctionDTO
    {
        public int AuctionId { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public VehicleDTO? Vehicle { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public List<BidDTO>? Bids { get; set; }
    }
}
