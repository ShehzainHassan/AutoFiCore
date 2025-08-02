using AutoFiCore.Enums;

public class TrackEventRequest
{
    public AnalyticsEventType Type { get; set; }
    public int UserId { get; set; }
    public int AuctionId { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}
