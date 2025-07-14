using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoFiCore.Models
{
    public class Auction
    {
        [Key]
        public int AuctionId { get; set; }

        [Required]
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        [Required]
        public DateTime StartUtc { get; set; }
        [Required]
        public DateTime EndUtc { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StartingPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CurrentPrice { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<Watchlist> Watchers { get; set; } = new List<Watchlist>();

    }
}
