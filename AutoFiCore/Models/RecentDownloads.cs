using AutoFiCore.Enums;

namespace AutoFiCore.Models
{
    public class RecentDownloads
    {
        public int Id { get; set; }
        public ReportType ReportType { get; set; }
        public string DateRange { get; set; } = string.Empty;
        public string Format { get; set; } = "CSV";
        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }
}
