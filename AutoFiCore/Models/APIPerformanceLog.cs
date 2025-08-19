using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a log entry for tracking API performance metrics such as response time and status code.
/// </summary>
public class APIPerformanceLog
{
    /// <summary>
    /// The unique identifier for the log entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The API endpoint that was called (e.g., /api/auctions).
    /// </summary>
    [Required]
    public string Endpoint { get; set; } = null!;

    /// <summary>
    /// The time taken to receive a response from the API.
    /// </summary>
    [Required]
    public TimeSpan ResponseTime { get; set; }

    /// <summary>
    /// The HTTP status code returned by the API (e.g., 200, 404, 500).
    /// </summary>
    [Required]
    public int StatusCode { get; set; }

    /// <summary>
    /// The UTC timestamp when the log entry was created.
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}