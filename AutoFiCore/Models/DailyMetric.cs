using AutoFiCore.Enums;

namespace AutoFiCore.Models
{
    /// <summary>
    /// Represents a daily metric entry used for tracking performance, engagement, or system-level statistics.
    /// </summary>
    public class DailyMetric
    {
        /// <summary>
        /// Unique identifier for the metric record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The calendar date for which the metric is recorded.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// The type of metric being tracked (e.g., AuctionCount, BidCount).
        /// </summary>
        public MetricType MetricType { get; set; }

        /// <summary>
        /// Optional decimal value representing the metric (e.g., revenue, ratio).
        /// </summary>
        public decimal? Value { get; set; }

        /// <summary>
        /// Optional count value representing the metric (e.g., number of bids, users).
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// Optional category to classify the metric
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// UTC timestamp when the metric record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}