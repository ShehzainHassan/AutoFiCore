namespace AutoFiCore.Models
{
    public class AuctionWinners
    {
        public int AuctionId { get; set; }
        public int VehicleId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal WinningBid { get; set; }
        public DateTime WonAt { get; set; }
    }
}
