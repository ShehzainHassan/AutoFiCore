namespace AutoFiCore.Dto
{
    public class CreateAuctionDTO
    {
        public int VehicleId { get; set; }

        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public decimal StartingPrice { get; set; }
        
        public AuctionStatus? Status { get; set; }
    }
}
