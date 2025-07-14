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

    }
}
