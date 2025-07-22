using AutoFiCore.Data;
using AutoFiCore.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class AuctionScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuctionScheduler> _logger;

    public AuctionScheduler(IServiceScopeFactory scopeFactory, ILogger<AuctionScheduler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auction Scheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var nowUtc = DateTime.UtcNow;

                // Scheduled -> PreviewMode
                var toPreview = await dbContext.Auctions
                    .Where(a => a.Status == AuctionStatus.Scheduled
                        && a.PreviewStartTime.HasValue
                        && a.PreviewStartTime.Value <= nowUtc)
                    .ToListAsync(stoppingToken);

                foreach (var auction in toPreview)
                {
                    auction.Status = AuctionStatus.PreviewMode;
                    _logger.LogInformation($"Auction {auction.AuctionId} moved to PreviewMode at {nowUtc}.");
                    // TODO: Send notification or SignalR event for preview start
                }

                // Scheduled -> Active (if no preview time)
                var toActivateDirectly = await dbContext.Auctions
                    .Where(a => a.Status == AuctionStatus.Scheduled
                        && !a.PreviewStartTime.HasValue
                        && a.ScheduledStartTime <= nowUtc)
                    .ToListAsync(stoppingToken);

                foreach (var auction in toActivateDirectly)
                {
                    auction.Status = AuctionStatus.Active;
                    auction.StartUtc = nowUtc;
                    _logger.LogInformation($"Auction {auction.AuctionId} (no preview) activated at {nowUtc}.");
                    // TODO: Send notification or SignalR event for auction start
                }

                // PreviewMode -> Active
                var toActivate = await dbContext.Auctions
                    .Where(a => a.Status == AuctionStatus.PreviewMode
                        && a.ScheduledStartTime <= nowUtc)
                    .ToListAsync(stoppingToken);

                foreach (var auction in toActivate)
                {
                    auction.Status = AuctionStatus.Active;
                    auction.StartUtc = nowUtc;
                    _logger.LogInformation($"Auction {auction.AuctionId} moved from PreviewMode to Active at {nowUtc}.");
                    // TODO: Send notification or SignalR event for auction start
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AuctionScheduler.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
