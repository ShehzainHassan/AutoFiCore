using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a log entry for a database query, capturing its type, execution duration, and timestamp.
/// </summary>
public class DBQueryLog
{
    /// <summary>
    /// Unique identifier for the log entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Type of query executed (e.g., SELECT, INSERT, UPDATE, DELETE).
    /// </summary>
    [Required(ErrorMessage = "QueryType is required.")]
    public string QueryType { get; set; } = null!;

    /// <summary>
    /// Duration of the query execution.
    /// </summary>
    [Required(ErrorMessage = "Duration is required.")]
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// UTC timestamp when the query was executed.
    /// </summary>
    [Required(ErrorMessage = "Timestamp is required.")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}