namespace AutoFiCore.Dto
{
    public class AIServiceSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; }
        public int MaxRetryAttempts { get; set; }
        public int CircuitBreakerThreshold { get; set; }
    }
}
