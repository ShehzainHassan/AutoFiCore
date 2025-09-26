public class SystemHealthDashboard
{
    public double AverageApiResponseTime { get; set; }
    public double AverageApiResponseTimeChange { get; set; }

    public double ErrorRate { get; set; }
    public double ErrorRateChange { get; set; }

    public int ActiveSessions { get; set; }
    public int ActiveSessionsChange { get; set; }

    public double SystemUptime { get; set; }
}