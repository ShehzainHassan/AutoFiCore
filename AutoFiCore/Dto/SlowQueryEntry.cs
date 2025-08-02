public class SlowQueryEntry
{
    public string QueryType { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime Timestamp { get; set; }
}