using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using AutoFiCore.Constants;
using AutoFiCore.Enums;

namespace AutoFiCore.Models
{
    public class Auction
    {
        [Key]
        public int AuctionId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Vehicle Vehicle { get; set; } = null!;

        [Required]
        public DateTime StartUtc { get; set; }
        [Required]
        public DateTime EndUtc { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StartingPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CurrentPrice { get; set; }
        public decimal? ReservePrice { get; set; }
        public bool IsReserveMet { get; set; } = false;
        public DateTime? ReserveMetAt { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AuctionStatus Status { get; set; } = AuctionStatus.Active;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime ScheduledStartTime { get; set; }
        public DateTime PreviewStartTime { get; set; }
        public int ExtensionMinutes { get; set; } = AuctionDefaults.TIME_TO_EXTEND;
        public int TriggerMinutes { get; set; } = AuctionDefaults.TRIGGER_TIME;
        public int ExtensionCount { get; set; } = 0;
        public int MaxExtensions { get; set; } = AuctionDefaults.MAX_EXTENSIONS;
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<Watchlist> Watchers { get; set; } = new List<Watchlist>();
        public ICollection<AutoBid> AutoBids { get; set; } = new List<AutoBid>();
        public ICollection<BidStrategy> BidStrategies { get; set; } = new List<BidStrategy>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual AuctionAnalytics? AuctionAnalytics { get; set; }

    }
}
