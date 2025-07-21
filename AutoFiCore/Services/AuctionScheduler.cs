//using AutoFiCore.Data;
//using AutoFiCore.Enums;
//using Microsoft.EntityFrameworkCore;

//using System;

//public class AuctionScheduler : BackgroundService
//{
//    private readonly IServiceScopeFactory _scopeFactory;
//    private readonly ILogger<AuctionScheduler> _logger;

//    public AuctionScheduler(IServiceScopeFactory scopeFactory, ILogger<AuctionScheduler> logger)
//    {
//        _scopeFactory = scopeFactory;
//        _logger = logger;
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            try
//            {
//                using var scope = _scopeFactory.CreateScope();
//                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//                var now = DateTime.UtcNow;

//                var toPreview = await dbContext.Auctions
//                    .Where(a => a.Status == AuctionStatus.Scheduled &&
//                                a.PreviewStartTime <= now)
//                    .ToListAsync(stoppingToken);

//                foreach (var auction in toPreview)
//                {
//                    auction.Status = AuctionStatus.PreviewMode;
//                    _logger.LogInformation($"Auction {auction.AuctionId} moved to PreviewMode.");
//                }

//                var toActivate = await dbContext.Auctions
//                    .Where(a => a.Status == AuctionStatus.PreviewMode &&
//                                a.ScheduledStartTime <= now)
//                    .ToListAsync(stoppingToken);

//                foreach (var auction in toActivate)
//                {
//                    auction.Status = AuctionStatus.Active;
//                    auction.StartUtc = now;
//                    _logger.LogInformation($"Auction {auction.AuctionId} activated.");
//                }

//                await dbContext.SaveChangesAsync(stoppingToken);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error running AuctionScheduler.");
//            }

//            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
//        }
//    }
//}
