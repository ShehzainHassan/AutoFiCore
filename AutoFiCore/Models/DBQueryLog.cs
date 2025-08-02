using System.ComponentModel.DataAnnotations;

public class DBQueryLog
{
    public int Id { get; set; }

    [Required]
    public string QueryType { get; set; } = null!;

    [Required]
    public TimeSpan Duration { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
