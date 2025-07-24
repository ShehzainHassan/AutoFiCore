using AutoFiCore.Data;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace AutoFiCore.BackgroundServices
{
    public class AutoBidBackgroundService : BackgroundService
    {
        private readonly ILogger<AutoBidBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public AutoBidBackgroundService(ILogger<AutoBidBackgroundService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AutoBid background service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running auto-bid check... at {Time}", DateTime.UtcNow);

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var autoBidService = scope.ServiceProvider.GetRequiredService<IAutoBidService>();

                        var activeAuctions = await uow.Auctions.GetAuctionsWithActiveAutoBidsAsync();
                     
                        if (activeAuctions.Count == 0)
                        {
                            _logger.LogInformation("No active autobids found");
                            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                            continue;
                        }
                        foreach (var auction in activeAuctions)
                        {
                            decimal highestBid = await uow.Bids.GetHighestBidAmountAsync(auction.AuctionId, auction.StartingPrice);
                            await autoBidService.ProcessAutoBidTrigger(auction.AuctionId, highestBid);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing auto-bids");
                }
            }

            _logger.LogInformation("AutoBid background service is stopping.");
        }
    }
}

