using Microsoft.Extensions.Diagnostics.HealthChecks;
using AutoFiCore.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Performs a health check on the database by verifying that critical tables are reachable.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// List of critical tables to check.
    /// </summary>
    private readonly string[] _criticalTables = new[]
    {
        "Vehicles",
        "Auctions",
        "Users",
        "Bids",
        "AutoBids",
        "BidStrategies",
        "Notifications",
        "Watchlists",
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseHealthCheck"/> class.
    /// </summary>
    /// <param name="dbContext">The application's database context.</param>
    public DatabaseHealthCheck(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Attempts to query a single row from the specified table to verify reachability.
    /// </summary>
    /// <param name="tableName">The name of the table to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the table is reachable; otherwise, false.</returns>
    private async Task<bool> IsTableReachableAsync(string tableName, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync($"SELECT 1 FROM [{tableName}]", cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks the health of the database by ensuring critical tables are reachable.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="HealthCheckResult"/> indicating the database health.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var failedTables = new List<string>();

        foreach (var table in _criticalTables)
        {
            if (!await IsTableReachableAsync(table, cancellationToken))
            {
                failedTables.Add(table);
            }
        }

        if (failedTables.Any())
        {
            var failedList = string.Join(", ", failedTables);
            return HealthCheckResult.Unhealthy($"Database check failed for tables: {failedList}");
        }

        return HealthCheckResult.Healthy("All critical tables are reachable");
    }
}
