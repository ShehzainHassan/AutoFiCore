public class APIResponseStat
{
    public string Endpoint { get; set; } = string.Empty;
    public double AverageResponseTimeMs { get; set; }
    public int RequestCount { get; set; }
}