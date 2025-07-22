using System.Text.Json.Serialization;
using AutoFiCore.Enums;

namespace AutoFiCore.Dto
{
    public class AuctionDTO
    {
        public int AuctionId { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal? ReservePrice { get; set; }
        public decimal CurrentPrice { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AuctionStatus Status { get; set; } = AuctionStatus.Active;
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime? PreviewStartTime { get; set; }
        public VehicleDTO? Vehicle { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public List<BidDTO>? Bids { get; set; }
    }
}
