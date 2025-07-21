namespace AutoFiCore.Models;

/// <summary>
/// Configuration settings for CORS (Cross-Origin Resource Sharing)
/// </summary>
public class CorsSettings
{
    /// <summary>
    /// List of allowed origins for CORS requests
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = new();

    /// <summary>
    /// Gets the allowed origins, including environment variable overrides
    /// </summary>
    /// <returns>List of allowed origins with environment variable additions</returns>
    public List<string> GetAllowedOrigins()
    {
        var origins = new List<string>(AllowedOrigins);
        
        // Add origins from environment variable (comma-separated)
        var envOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
        if (!string.IsNullOrEmpty(envOrigins))
        {
            var additionalOrigins = envOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .Where(o => !string.IsNullOrEmpty(o));
            
            origins.AddRange(additionalOrigins);
        }
        
        return origins.Distinct().ToList();
    }
} 