using System.ComponentModel.DataAnnotations;

public class APIPerformanceLog
{
    public int Id { get; set; }

    [Required]
    public string Endpoint { get; set; } = null!;

    [Required]
    public TimeSpan ResponseTime { get; set; }

    [Required]
    public int StatusCode { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
