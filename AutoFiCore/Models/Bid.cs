using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoFiCore.Models
{
    /// <summary>
    /// Represents a bid placed by a user in an auction, either manually or via auto-bidding.
    /// </summary>
    public class Bid
    {
        /// <summary>
        /// Unique identifier for the bid.
        /// </summary>
        [Key]
        public int BidId { get; set; }

        /// <summary>
        /// ID of the auction this bid belongs to.
        /// </summary>
        [Required]
        public int AuctionId { get; set; }

        /// <summary>
        /// Navigation property for the associated auction.
        /// </summary>
        public Auction Auction { get; set; } = null!;

        /// <summary>
        /// ID of the user who placed the bid.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property for the bidding user.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// The amount of the bid. Must be greater than zero.
        /// </summary>
        [Range(1, double.MaxValue, ErrorMessage = "Bid amount must be positive.")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Indicates whether the bid was placed automatically.
        /// </summary>
        public bool IsAuto { get; set; } = false;

        /// <summary>
        /// UTC timestamp when the bid was created.
        /// </summary>
        [Required]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional timing preference for when the bid should be placed.
        /// </summary>
        public PreferredBidTiming? PreferredBidTiming { get; set; }
    }
}