namespace AutoFiCore.Dto
{
    
        public class AuctionResponseDTO
        {
            public int AuctionId { get; set; }
            public int VehicleId { get; set; }
            public DateTime StartUtc { get; set; }
            public DateTime EndUtc { get; set; }
            public decimal StartingPrice { get; set; }
            public decimal CurrentPrice { get; set; }
            public string Status { get; set; } = string.Empty;
        }

}
