namespace AutoFiCore.Dto
{
    public class BidDTO
    {
        public int BidId { get; set; }
        public int AuctionId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public bool IsAuto { get; set; } = false;
        public DateTime PlacedAt { get; set; }
    }
}
