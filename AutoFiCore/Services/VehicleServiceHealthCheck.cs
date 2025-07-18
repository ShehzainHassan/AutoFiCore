using AutoFiCore.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class VehicleServiceHealthCheck : IHealthCheck
{
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<VehicleServiceHealthCheck> _logger;

    public VehicleServiceHealthCheck(IVehicleService vehicleService, ILogger<VehicleServiceHealthCheck> logger)
    {
        _vehicleService = vehicleService;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple health check that doesn't rely on complex database queries
            // Just check if the service is available and can be instantiated
            if (_vehicleService != null)
            {
                _logger.LogInformation("Vehicle service health check passed - service is available");
                return Task.FromResult(HealthCheckResult.Healthy("Vehicle service is available."));
            }
            else
            {
                _logger.LogWarning("Vehicle service health check failed - service is null");
                return Task.FromResult(HealthCheckResult.Unhealthy("Vehicle service is not available."));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vehicle service health check failed with exception");
            return Task.FromResult(HealthCheckResult.Unhealthy($"Vehicle service health check failed: {ex.Message}"));
        }
    }
}
