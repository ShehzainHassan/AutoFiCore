using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(25)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<Watchlist> Watchlists { get; set; } = new List<Watchlist>();
        public ICollection<AutoBid> AutoBids { get; set; } = new List<AutoBid>();
        public ICollection<BidStrategy> BidStrategies { get; set; } = new List<BidStrategy>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();


    }
}
