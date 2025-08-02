using AutoFiCore.Enums;

namespace AutoFiCore.Models
{
    public class DailyMetric
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }

        public MetricType MetricType { get; set; }

        public decimal? Value { get; set; }
        public int? Count { get; set; }

        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
