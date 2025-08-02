
public class PerformanceReport
{
    public List<APIResponseStat> ApiStats { get; set; } = new();
    public int TotalSlowQueries { get; set; }
}