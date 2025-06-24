using AutoFiCore.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class VehicleServiceHealthCheck : IHealthCheck
{
    private readonly IVehicleService _vehicleService;

    public VehicleServiceHealthCheck(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var makes = await _vehicleService.GetAllVehiclesMakesAsync();

        if (makes != null && makes.Count > 0)
        {
            return HealthCheckResult.Healthy("Vehicle service is healthy.");
        }
        else
        {
            return HealthCheckResult.Degraded("Vehicle service is reachable but returned no data.");
        }
    }
}
