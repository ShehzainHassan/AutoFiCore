using AutoFiCore.Enums;

namespace AutoFiCore.Models
{
    /// <summary>
    /// Represents a tracked analytics event within the system, such as user actions or auction interactions.
    /// </summary>
    public class AnalyticsEvent
    {
        /// <summary>
        /// The unique identifier for the analytics event.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The type of event being tracked (e.g., AuctionView, BidPlaced etc).
        /// </summary>
        public AnalyticsEventType EventType { get; set; }

        /// <summary>
        /// The ID of the user associated with the event, if applicable.
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// The user entity associated with the event, if available.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// The ID of the auction associated with the event, if applicable.
        /// </summary>
        public int? AuctionId { get; set; }

        /// <summary>
        /// The auction entity associated with the event, if available.
        /// </summary>
        public Auction? Auction { get; set; }

        /// <summary>
        /// Optional metadata or payload describing the event (e.g., browser info, bid amount).
        /// </summary>
        public string? EventData { get; set; }

        /// <summary>
        /// The source of the event (e.g., Web, Mobile, API).
        /// </summary>
        public AnalyticsSource? Source { get; set; }

        /// <summary>
        /// The UTC timestamp when the event was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}