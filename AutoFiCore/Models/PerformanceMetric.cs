namespace AutoFiCore.Models
{
    public class PerformanceMetric
    {
        public int Id { get; set; }

        public string MetricType { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;

        public int ResponseTime { get; set; } 
        public int StatusCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
