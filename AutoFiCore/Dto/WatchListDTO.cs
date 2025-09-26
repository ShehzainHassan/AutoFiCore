namespace AutoFiCore.Dto
{
    public class WatchlistDTO
    {
        public int WatchlistId { get; set; }
        public int UserId { get; set; }
        public int AuctionId { get; set; }
        public int VehicleId { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

}
