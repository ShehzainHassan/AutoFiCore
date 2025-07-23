using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public interface IAuctionSchedulerService
{
    Task<Result<CreateAuctionDTO>> UpdateScheduledAuctionAsync(int auctionId, CreateAuctionDTO dto);
}

public class AuctionScheduler : BackgroundService, IAuctionSchedulerService
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
                    .Where(a => a.Status == AuctionStatus.Scheduled && a.PreviewStartTime <= nowUtc)
                    .ToListAsync(stoppingToken);

                foreach (var auction in toPreview)
                {
                    auction.Status = AuctionStatus.PreviewMode;
                    _logger.LogInformation("Auction {AuctionId} moved to PreviewMode at {Time}", auction.AuctionId, nowUtc);
                    // TODO: Send SignalR preview start event
                }

                // Scheduled -> Active
                var toActivateDirectly = await dbContext.Auctions
                    .Where(a => a.Status == AuctionStatus.Scheduled && a.ScheduledStartTime <= nowUtc)
                    .ToListAsync(stoppingToken);

                foreach (var auction in toActivateDirectly)
                {
                    auction.Status = AuctionStatus.Active;
                    auction.StartUtc = nowUtc;
                    _logger.LogInformation("Auction {AuctionId} moved to Active at {Time}", auction.AuctionId, nowUtc);
                    // TODO: Send SignalR auction start event
                }

                // PreviewMode -> Active
                var toActivate = await dbContext.Auctions
                    .Where(a => a.Status == AuctionStatus.PreviewMode && a.ScheduledStartTime <= nowUtc)
                    .ToListAsync(stoppingToken);

                foreach (var auction in toActivate)
                {
                    auction.Status = AuctionStatus.Active;
                    auction.StartUtc = nowUtc;
                    _logger.LogInformation("Auction {AuctionId} moved from PreviewMode to Active at {Time}", auction.AuctionId, nowUtc);
                    // TODO: Send SignalR auction start event
                }

                // Active -> Ended
                var toEnd = await dbContext.Auctions
                    .Where(a => a.Status == AuctionStatus.Active && a.EndUtc <= nowUtc)
                    .ToListAsync(stoppingToken);

                foreach (var auction in toEnd)
                {
                    auction.Status = AuctionStatus.Ended;
                    _logger.LogInformation("Auction {AuctionId} ended at {Time}", auction.AuctionId, nowUtc);
                    // TODO: Send SignalR auction end event
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

    public async Task<Result<CreateAuctionDTO>> UpdateScheduledAuctionAsync(int auctionId, CreateAuctionDTO dto)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var auction = await uow.Auctions.GetAuctionByIdAsync(auctionId);
        if (auction == null)
            return Result<CreateAuctionDTO>.Failure("Auction not found");

        bool vehicleExists = uow.Vehicles.VehicleExists(dto.VehicleId);
        if (!vehicleExists)
            return Result<CreateAuctionDTO>.Failure("Vehicle not found");

        if (auction.VehicleId != dto.VehicleId &&
            await uow.Auctions.VehicleHasAuction(dto.VehicleId))
        {
            return Result<CreateAuctionDTO>.Failure("An auction already exists for this vehicle");
        }

        if (DateTime.UtcNow >= auction.PreviewStartTime)
            return Result<CreateAuctionDTO>.Failure("Cannot update auction after PreviewStartTime");

        var errors = Validator.ValidateAuctionDto(dto);
        if (errors.Any())
            return Result<CreateAuctionDTO>.Failure(string.Join("; ", errors));

        var now = DateTime.UtcNow;
        var isFutureStart = dto.StartUtc > now;
        DateTime previewTime = dto.PreviewStartTime ?? dto.StartUtc;
        dto.PreviewStartTime = previewTime;
        bool hasPreviewStarted = previewTime <= now;

        var newStatus = isFutureStart
            ? (hasPreviewStarted ? AuctionStatus.PreviewMode : AuctionStatus.Scheduled)
            : AuctionStatus.Active;

        decimal reserve = dto.ReservePrice ?? dto.StartingPrice;
        bool reserveMet = auction.StartingPrice >= reserve;
        DateTime? reserveMetAt = reserveMet ? now : null;

        auction.VehicleId = dto.VehicleId;
        auction.StartUtc = dto.StartUtc;
        auction.ScheduledStartTime = dto.StartUtc;
        auction.EndUtc = dto.EndUtc;
        auction.PreviewStartTime = previewTime;
        auction.StartingPrice = dto.StartingPrice;
        auction.ReservePrice = dto.ReservePrice;
        auction.IsReserveMet = reserveMet;
        auction.ReserveMetAt = reserveMetAt;
        auction.UpdatedUtc = now;
        auction.Status = newStatus;

        uow.Auctions.UpdateAuction(auction);
        await uow.SaveChangesAsync();

        return Result<CreateAuctionDTO>.Success(dto);
    }

}
