namespace AutoFiCore.Models;

/// <summary>
/// Configuration class for database settings
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Connection string for the database
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of retry attempts for database operations
    /// </summary>
    public int MaxRetryCount { get; set; }

    /// <summary>
    /// Delay in seconds between retry attempts
    /// </summary>
    public int RetryDelaySeconds { get; set; }

    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeoutSeconds { get; set; }
}