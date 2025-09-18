namespace AutoFiCore.Configurations
{
    public class RateLimitSettings
    {
        public RateLimitPolicy Global { get; set; } = new();
        public RateLimitPolicy AI { get; set; } = new();
    }

    public class RateLimitPolicy
    {
        public int PermitLimit { get; set; }
        public int WindowSeconds { get; set; }
        public int QueueLimit { get; set; }
    }
}
