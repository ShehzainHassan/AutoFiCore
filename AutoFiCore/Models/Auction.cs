using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AutoFiCore.Constants;
using AutoFiCore.Enums;

namespace AutoFiCore.Models
{
    /// <summary>
    /// Represents an auction listing for a vehicle, including pricing, timing, and bidding metadata.
    /// </summary>
    public class Auction
    {
        /// <summary>
        /// The unique identifier for the auction.
        /// </summary>
        [Key]
        public int AuctionId { get; set; }

        /// <summary>
        /// The ID of the vehicle being auctioned.
        /// </summary>
        [Required]
        public int VehicleId { get; set; }

        /// <summary>
        /// The vehicle associated with the auction.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Vehicle Vehicle { get; set; } = null!;

        /// <summary>
        /// The UTC timestamp when the auction starts.
        /// </summary>
        [Required]
        public DateTime StartUtc { get; set; }

        /// <summary>
        /// The UTC timestamp when the auction ends.
        /// </summary>
        [Required]
        public DateTime EndUtc { get; set; }

        /// <summary>
        /// The initial price at which bidding begins.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal StartingPrice { get; set; }

        /// <summary>
        /// The current highest bid price.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// The minimum price required to sell the vehicle. Optional.
        /// </summary>
        public decimal? ReservePrice { get; set; }

        /// <summary>
        /// Indicates whether the reserve price has been met.
        /// </summary>
        public bool IsReserveMet { get; set; } = false;

        /// <summary>
        /// The UTC timestamp when the reserve price was met, if applicable.
        /// </summary>
        public DateTime? ReserveMetAt { get; set; }

        /// <summary>
        /// The current status of the auction (e.g., Active, Ended).
        /// </summary>
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AuctionStatus Status { get; set; } = AuctionStatus.Active;

        /// <summary>
        /// The UTC timestamp when the auction was created.
        /// </summary>
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The UTC timestamp when the auction was last updated.
        /// </summary>
        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The scheduled start time for the auction, used for pre-launch visibility.
        /// </summary>
        public DateTime ScheduledStartTime { get; set; }

        /// <summary>
        /// The time when the auction preview becomes visible to users.
        /// </summary>
        public DateTime PreviewStartTime { get; set; }

        /// <summary>
        /// The number of minutes to extend the auction if a bid is placed near the end.
        /// </summary>
        public int ExtensionMinutes { get; set; } = AuctionDefaults.TIME_TO_EXTEND;

        /// <summary>
        /// The number of minutes before auction end that triggers an extension.
        /// </summary>
        public int TriggerMinutes { get; set; } = AuctionDefaults.TRIGGER_TIME;

        /// <summary>
        /// The number of times the auction has been extended.
        /// </summary>
        public int ExtensionCount { get; set; } = 0;

        /// <summary>
        /// The maximum number of extensions allowed for the auction.
        /// </summary>
        public int MaxExtensions { get; set; } = AuctionDefaults.MAX_EXTENSIONS;

        /// <summary>
        /// The collection of bids placed during the auction.
        /// </summary>
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();

        /// <summary>
        /// The collection of users who have added the auction to their watchlist.
        /// </summary>
        public ICollection<Watchlist> Watchers { get; set; } = new List<Watchlist>();

        /// <summary>
        /// The collection of auto-bid configurations associated with the auction.
        /// </summary>
        public ICollection<AutoBid> AutoBids { get; set; } = new List<AutoBid>();

        /// <summary>
        /// The collection of bid strategies applied to the auction.
        /// </summary>
        public ICollection<BidStrategy> BidStrategies { get; set; } = new List<BidStrategy>();

        /// <summary>
        /// The collection of notifications triggered by auction events.
        /// </summary>
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        /// <summary>
        /// The analytics data associated with the auction, if available.
        /// </summary>
        public virtual AuctionAnalytics? AuctionAnalytics { get; set; }
    }
}