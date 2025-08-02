using System.ComponentModel.DataAnnotations;

public class ErrorLog
{
    public int Id { get; set; }

    [Required]
    public string ErrorType { get; set; } = null!;

    [Required]
    public string Message { get; set; } = null!;

    [Required]
    public string StackTrace { get; set; } = null!;

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
