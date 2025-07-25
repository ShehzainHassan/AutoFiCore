using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoFiCore.Models
{
    public class Bid
    {
        [Key]
        public int BidId { get; set; }
        [Required] 
        public int AuctionId { get; set; }
        public Auction Auction { get; set; } = null!;
        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        [Range(1, double.MaxValue)]
        public decimal Amount { get; set; }
        public bool IsAuto { get; set; } = false;
        [Required]
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public PreferredBidTiming? PreferredBidTiming { get; set; }
    }

}
