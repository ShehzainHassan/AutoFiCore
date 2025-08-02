namespace AutoFiCore.Dto
{
    public class HourlyPerformanceMetrics
    {
        public DateTime Hour { get; set; }
        public double AvgApiResponseTime { get; set; }
        public int TotalApiCalls { get; set; }
        public int SlowQueryCount { get; set; }
    }

}
