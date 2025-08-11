using System.ComponentModel.DataAnnotations;

public class ErrorLog
{
    public int Id { get; set; }

    [Required]
    public int ErrorCode { get; set; }

    [Required]
    public string Message { get; set; } = null!;

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
