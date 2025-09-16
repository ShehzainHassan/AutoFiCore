using AutoFiCore.Dto;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http.Json;

public class FastApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FastApiHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("FastApi");
            var response = await client.GetFromJsonAsync<FastApiHealthResponse>("health", cancellationToken);

            if (response == null)
                return HealthCheckResult.Unhealthy("No response from FastAPI health endpoint");

            if (!response.db)
                return HealthCheckResult.Unhealthy("FastAPI DB not ready");

            if (!response.orchestrator_ready)
                return HealthCheckResult.Degraded("Orchestrator not ready");

            if (!response.ml_models_loaded)
                return HealthCheckResult.Degraded("ML models not loaded yet");

            return HealthCheckResult.Healthy("FastAPI is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("FastAPI health check failed", ex);
        }
    }
}