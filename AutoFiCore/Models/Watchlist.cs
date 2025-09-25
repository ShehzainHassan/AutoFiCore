using AutoFiCore.Models;
using System.ComponentModel.DataAnnotations;

public class Watchlist
{
    [Key]
    public int WatchlistId { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    [Required] public int AuctionId { get; set; }
    public Auction Auction { get; set; } = null!;

    [Required]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
