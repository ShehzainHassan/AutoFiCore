namespace AutoFiCore.Dto
{
    public class UserActivityReport
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewRegistrations { get; set; }
        public double RetentionRate { get; set; }
        public decimal EngagementScore { get; set; }
    }
}
