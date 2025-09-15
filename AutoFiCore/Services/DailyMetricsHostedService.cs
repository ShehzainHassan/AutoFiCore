using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFiCore.Services;
using AutoFiCore.Data.Interfaces;

public class DailyMetricsHostedService : BackgroundService
{
    private readonly ILogger<DailyMetricsHostedService> _logger;
    private readonly IServiceProvider _services;

    public DailyMetricsHostedService(ILogger<DailyMetricsHostedService> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextRunTime = now.Date.AddDays(1);
                var delay = nextRunTime - now;

                _logger.LogInformation("Daily metrics will run in: {delay}", delay);

                await Task.Delay(delay, stoppingToken);

                using var scope = _services.CreateScope();
                var metricsService = scope.ServiceProvider.GetRequiredService<IMetricsCalculationService>();

                await metricsService.CalculateDailyMetricsAsync(DateTime.UtcNow.Date);

                _logger.LogInformation("Daily metrics calculated at: {time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during daily metrics calculation");
            }
        }
    }
}
