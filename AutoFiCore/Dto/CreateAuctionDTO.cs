using AutoFiCore.Enums;

namespace AutoFiCore.Dto
{
    public class CreateAuctionDTO
    {
        public int VehicleId { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime EndUtc { get; set; }
        public DateTime? PreviewStartTime { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal? ReservePrice { get; set; }
    }
}
