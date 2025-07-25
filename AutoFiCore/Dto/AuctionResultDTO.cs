public class AuctionResultDTO
{
    public bool IsReserveMet { get; set; }
    public bool IsSold { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; } = String.Empty;
    public decimal? WinningBid { get; set; }
    public int BidCount { get; set; }
}
