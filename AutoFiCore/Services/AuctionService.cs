using AutoFiCore.Dto;
using AutoFiCore.Mappers;
using AutoFiCore.Models;
using AutoFiCore.Queries;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;
using AutoFiCore.Enums;
using System;
using Microsoft.AspNetCore.SignalR;
using AutoFiCore.Hubs;
using AutoFiCore.Data.Interfaces;

namespace AutoFiCore.Services
{

    public class AuctionService : IAuctionService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AuctionService> _logger;
        private readonly IAuctionLifecycleService _auctionLifecycleService;
        public AuctionService(IUnitOfWork uow, ILogger<AuctionService> log, IAuctionLifecycleService auctionLifecycleService)
        {
            _uow = uow;
            _logger = log;
            _auctionLifecycleService = auctionLifecycleService;
        }
        public async Task<Result<AuctionDTO>> CreateAuctionAsync(CreateAuctionDTO dto)
        {
            var strategy = _uow.DbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await _uow.BeginTransactionAsync();
                    try
                    {
                        var vehicle = await _uow.Vehicles.GetVehicleByIdAsync(dto.VehicleId);
                        if (vehicle == null)
                            return Result<AuctionDTO>.Failure($"Vehicle {dto.VehicleId} not found");

                        if (await _uow.Auctions.VehicleHasAuction(dto.VehicleId))
                            return Result<AuctionDTO>.Failure("An auction already exists for this vehicle");

                        var now = DateTime.UtcNow;
                        var previewTime = dto.PreviewStartTime ?? dto.ScheduledStartTime;

                        AuctionStatus status;
                        if (dto.ScheduledStartTime > now)
                            status = previewTime <= now ? AuctionStatus.PreviewMode : AuctionStatus.Scheduled;
                        else
                            status = AuctionStatus.Active;

                        decimal reservePrice = dto.ReservePrice ?? dto.StartingPrice;
                        bool isReserveMet = reservePrice <= dto.StartingPrice;
                        DateTime? reserveMetAt = isReserveMet ? now : null;

                        var auction = new Auction
                        {
                            VehicleId = dto.VehicleId,
                            ScheduledStartTime = dto.ScheduledStartTime,
                            StartUtc = dto.ScheduledStartTime,
                            EndUtc = dto.EndUtc,
                            StartingPrice = dto.StartingPrice,
                            CurrentPrice = 0,
                            ReservePrice = reservePrice,
                            IsReserveMet = isReserveMet,
                            ReserveMetAt = reserveMetAt,
                            Status = status,
                            PreviewStartTime = previewTime,
                            CreatedUtc = now,
                            UpdatedUtc = now
                        };

                        await _uow.Auctions.AddAuctionAsync(auction);
                        await _uow.SaveChangesAsync();
                        await _uow.CommitTransactionAsync();

                        var dtoResult = AuctionMapper.ToDTO(auction);
                        return Result<AuctionDTO>.Success(dtoResult);
                    }
                    catch (Exception ex)
                    {
                        await _uow.RollbackTransactionAsync();
                        _logger.LogError(ex, "Failed to create auction for Vehicle {VehicleId}", dto.VehicleId);
                        return Result<AuctionDTO>.Failure("Failed to create auction.");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution strategy failed while creating auction for Vehicle {VehicleId}", dto.VehicleId);
                return Result<AuctionDTO>.Failure("Unexpected error occurred while creating auction.");
            }
        }
        public async Task<Result<AuctionDTO?>> UpdateAuctionStatusAsync(int auctionId, AuctionStatus status)
        {
            var strategy = _uow.DbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await _uow.BeginTransactionAsync();
                    try
                    {
                        var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
                        if (auction == null)
                        {
                            await _uow.RollbackTransactionAsync();
                            return Result<AuctionDTO?>.Failure("Auction not found.");
                        }

                        await _uow.Auctions.UpdateAuctionStatusAsync(auctionId, status);
                        await _uow.SaveChangesAsync();
                        await _uow.CommitTransactionAsync();

                        return Result<AuctionDTO?>.Success(AuctionMapper.ToDTO(auction));
                    }
                    catch (Exception ex)
                    {
                        await _uow.RollbackTransactionAsync();
                        _logger.LogError(ex, "Failed to update auction {AuctionId} to status {Status}", auctionId, status);
                        return Result<AuctionDTO?>.Failure("Failed to update auction status.");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution strategy failed while updating auction {AuctionId}", auctionId);
                return Result<AuctionDTO?>.Failure("Unexpected error occurred while updating auction status.");
            }
        }
        public async Task<Result<List<AuctionDTO>>> GetAuctionsAsync(AuctionQueryParams filters)
        {
            try
            {
                var query = _uow.Auctions.Query()
                    .Include(a => a.Vehicle)
                    .Include(a => a.Bids);

                var filteredQuery = AuctionQuery.ApplyFilters(query, filters);
                var sortedQuery = AuctionQuery.ApplySorting(filteredQuery, filters);

                //Not showing scheduled auctions, if auction has preview time it will be shown in preview mode
                var auctions = await sortedQuery
                    .Where(a => a.Status != AuctionStatus.Scheduled)
                    .AsNoTracking()
                    .ToListAsync();

                var dtoList = auctions.Select(AuctionMapper.ToDTO).ToList();

                return Result<List<AuctionDTO>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving auctions with filters: {@Filters}", filters);
                return Result<List<AuctionDTO>>.Failure("Failed to retrieve auctions.");
            }
        }
        public async Task<Result<AuctionDTO?>> GetAuctionByIdAsync(int id)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(id);

            if (auction == null)
                return Result<AuctionDTO?>.Failure("Auction not found.");

            return Result<AuctionDTO?>.Success(AuctionMapper.ToDTO(auction));
        }
        private bool ValidateBidAgainstReserve(Auction auction, decimal bidAmount)
        {
            return bidAmount >= auction.ReservePrice;
        }
        public async Task<Result<AuctionResultDTO?>> ProcessAuctionResultAsync(int auctionId)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            if (auction == null)
                return Result<AuctionResultDTO?>.Failure("Auction not found");

            if (auction.Status != AuctionStatus.Ended)
                return Result<AuctionResultDTO?>.Failure("Auction is not yet ended");

            var bids = await _uow.Bids.GetBidsByAuctionIdAsync(auctionId);

            if (bids.Count == 0)
            {
                return Result<AuctionResultDTO?>.Success(new AuctionResultDTO
                {
                    IsSold = false,
                    IsReserveMet = false,
                    UserId = null,
                    UserName = null,
                    WinningBid = null,
                    BidCount = 0,
                });
            }
            var highestBid = bids.OrderByDescending(b => b.Amount).First();

            var winningUser = await _uow.Users.GetUserByIdAsync(highestBid.UserId);
            if (winningUser == null)
                return Result<AuctionResultDTO?>.Failure("Winning user not found");

            bool reserveMet = ValidateBidAgainstReserve(auction, highestBid.Amount);

            if (reserveMet)
            {
                var existingWinner = await _uow.Auctions.GetAuctionWinnerAsync(
                    auction.AuctionId, winningUser.Id, auction.VehicleId);

                if (existingWinner == null)
                {
                    await _auctionLifecycleService.HandleAuctionWonAsync(auction, winningUser.Id);
                    await _uow.Auctions.AddAuctionWinnerAsync(winningUser.Id, auction.AuctionId, highestBid.Amount, auction.VehicleId, winningUser.Name);
                    await _uow.SaveChangesAsync();
                }
            }
            if (reserveMet)
                await _auctionLifecycleService.HandleAuctionWonAsync(auction, winningUser.Id);

            return Result<AuctionResultDTO?>.Success(new AuctionResultDTO
            {
                IsSold = reserveMet,
                IsReserveMet = reserveMet,
                UserId = reserveMet ? winningUser.Id : null,
                UserName = reserveMet ? winningUser.Name : null,
                WinningBid = reserveMet ? highestBid.Amount : null,
                BidCount = bids.Count
            });
        }
        public async Task<Result<BidDTO>> PlaceBidAsync(int auctionId, CreateBidDTO dto)
        {
            var strategy = _uow.DbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await _uow.BeginTransactionAsync();
                    try
                    {
                        var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
                        if (auction == null)
                            return Result<BidDTO>.Failure("Auction not found.");

                        var user = await _uow.Users.GetUserByIdAsync(dto.UserId);
                        if (user == null)
                            return Result<BidDTO>.Failure("User not found.");

                        if (auction.Status != AuctionStatus.Active || auction.EndUtc <= DateTime.UtcNow)
                            return Result<BidDTO>.Failure("Auction is not active or has already ended.");

                        var bids = await _uow.Bids.GetBidsByAuctionIdAsync(auctionId);
                        var previousBidders = await _uow.Bids.GetUniqueBidderIdsAsync(auctionId);
                        var previousHighestBidder = await _uow.Bids.GetHighestBidderIdAsync(auctionId);

                        var bidValidationDto = new BidValidationDTO
                        {
                            Amount = dto.Amount,
                            StartingPrice = auction.StartingPrice,
                            CurrentPrice = auction.CurrentPrice,
                            BidCount = bids.Count
                        };

                        var validator = new BidValidator();
                        var validationResult = validator.Validate(bidValidationDto);

                        if (!validationResult.IsValid)
                        {
                            await _uow.RollbackTransactionAsync();
                            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                            return Result<BidDTO>.Failure(errors);
                        }

                        PreferredBidTiming? timing = null;
                        if (dto.IsAuto == true)
                        {
                            var strategyEntity = await _uow.AutoBid.GetBidStrategyByUserAndAuctionAsync(dto.UserId, auctionId);
                            if (strategyEntity != null)
                                timing = strategyEntity.PreferredBidTiming;
                        }

                        var bid = new Bid
                        {
                            AuctionId = auctionId,
                            UserId = dto.UserId,
                            Amount = dto.Amount,
                            IsAuto = dto.IsAuto ?? false,
                            CreatedUtc = DateTime.UtcNow,
                            PreferredBidTiming = timing
                        };

                        await _uow.Bids.AddBidAsync(bid);
                        await _uow.Auctions.UpdateCurrentPriceAsync(auctionId, dto.Amount);

                        bool reserveJustMet = false;
                        if (auction.ReservePrice.HasValue && !auction.IsReserveMet && bid.Amount >= auction.ReservePrice.Value)
                        {
                            auction.IsReserveMet = true;
                            auction.ReserveMetAt = DateTime.UtcNow;
                            await _uow.Auctions.UpdateReserveStatusAsync(auctionId);
                            reserveJustMet = true;
                        }

                        var timeRemaining = auction.EndUtc - DateTime.UtcNow;
                        if (timeRemaining.TotalMinutes <= auction.TriggerMinutes && auction.ExtensionCount < auction.MaxExtensions)
                        {
                            await _uow.Auctions.UpdateAuctionEndTimeAsync(auctionId, auction.ExtensionMinutes);
                        }

                        await _uow.SaveChangesAsync();

                        var updatedBidders = await _uow.Bids.GetUniqueBidderIdsAsync(auctionId);

                        if (reserveJustMet)
                            await _auctionLifecycleService.HandleReserveMet(auction);
                        else if (auction.IsReserveMet)
                            await _auctionLifecycleService.HandleReserveMet(auction, bid.UserId);

                        await _auctionLifecycleService.HandleBidderCountUpdate(auction, previousBidders, updatedBidders);
                        await _auctionLifecycleService.HandleNewBid(auctionId);
                        await _auctionLifecycleService.HandleOutbid(auction, previousHighestBidder);

                        await _uow.CommitTransactionAsync();

                        var bidDto = new BidDTO
                        {
                            BidId = bid.BidId,
                            AuctionId = bid.AuctionId,
                            UserId = bid.UserId,
                            Amount = bid.Amount,
                            IsAuto = bid.IsAuto,
                            PlacedAt = bid.CreatedUtc
                        };

                        return Result<BidDTO>.Success(bidDto);
                    }
                    catch (Exception ex)
                    {
                        await _uow.RollbackTransactionAsync();
                        _logger.LogError(ex, "Failed to place bid on auction {AuctionId} by user {UserId}", auctionId, dto.UserId);
                        return Result<BidDTO>.Failure("Failed to place bid due to an unexpected error.");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution strategy failed while placing bid on auction {AuctionId}", auctionId);
                return Result<BidDTO>.Failure("Unexpected error occurred while placing bid.");
            }
        }
        public async Task<Result<List<BidDTO>>> GetBidHistoryAsync(int auctionId)
        {
            if (await _uow.Auctions.GetAuctionByIdAsync(auctionId) is null)
                return Result<List<BidDTO>>.Failure("Auction not found.");

            var bids = await _uow.Bids.GetBidsByAuctionIdAsync(auctionId);

            var dtos = bids.Select(b => new BidDTO
            {
                BidId = b.BidId,
                AuctionId = b.AuctionId,
                UserId = b.UserId,
                Amount = b.Amount,
                IsAuto = b.IsAuto,
                PlacedAt = b.CreatedUtc
            }).ToList();

            return Result<List<BidDTO>>.Success(dtos);
        }
        public async Task<Result<List<BidDTO>>> GetUserBidHistoryAsync(int userId)
        {
            if (await _uow.Users.GetUserByIdAsync(userId) is null)
                return Result<List<BidDTO>>.Failure("User not found.");

            var bids = await _uow.Bids.GetBidsByUserIdAsync(userId);

            var dtos = bids.Select(b => new BidDTO
            {
                BidId = b.BidId,
                AuctionId = b.AuctionId,
                UserId = b.UserId,
                Amount = b.Amount,
                IsAuto = b.IsAuto,
                PlacedAt = b.CreatedUtc
            }).ToList();

            return Result<List<BidDTO>>.Success(dtos);
        }
        public async Task<Result<string>> AddToWatchListAsync(int userId, int auctionId)
        {
            var strategy = _uow.DbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await _uow.BeginTransactionAsync();
                    try
                    {
                        var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
                        if (auction is null)
                            return Result<string>.Failure("Auction not found.");

                        if (await _uow.Users.GetUserByIdAsync(userId) is null)
                            return Result<string>.Failure("User not found.");

                        await _uow.Watchlist.AddToWatchlistAsync(userId, auctionId, auction.VehicleId);
                        await _uow.SaveChangesAsync();
                        await _uow.CommitTransactionAsync();

                        return Result<string>.Success("Auction added to watchlist.");
                    }
                    catch (Exception ex)
                    {
                        await _uow.RollbackTransactionAsync();
                        _logger.LogError(ex, "Failed to add auction {AuctionId} to watchlist for user {UserId}", auctionId, userId);
                        return Result<string>.Failure("Unexpected error while adding to watchlist.");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution strategy failed while adding auction {AuctionId} to watchlist", auctionId);
                return Result<string>.Failure("Unexpected error occurred.");
            }
        }
        public async Task<Result<string>> RemoveFromWatchListAsync(int userId, int auctionId)
        {
            var strategy = _uow.DbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await _uow.BeginTransactionAsync();
                    try
                    {
                        if (await _uow.Auctions.GetAuctionByIdAsync(auctionId) is null)
                            return Result<string>.Failure("Auction not found.");

                        if (await _uow.Users.GetUserByIdAsync(userId) is null)
                            return Result<string>.Failure("User not found.");

                        await _uow.Watchlist.RemoveFromWatchlistAsync(userId, auctionId);
                        await _uow.SaveChangesAsync();
                        await _uow.CommitTransactionAsync();

                        return Result<string>.Success("Removed auction from watchlist.");
                    }
                    catch (Exception ex)
                    {
                        await _uow.RollbackTransactionAsync();
                        _logger.LogError(ex, "Failed to remove auction {AuctionId} from watchlist for user {UserId}", auctionId, userId);
                        return Result<string>.Failure("Unexpected error while removing from watchlist.");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution strategy failed while removing auction {AuctionId} from watchlist", auctionId);
                return Result<string>.Failure("Unexpected error occurred.");
            }
        }
        public async Task<Result<List<WatchlistDTO>>> GetUserWatchListAsync(int userId)
        {
            if (await _uow.Users.GetUserByIdAsync(userId) is null)
                return Result<List<WatchlistDTO>>.Failure("User not found.");

            var watchlistDtos = await _uow.Watchlist.GetUserWatchlistAsync(userId);

            return Result<List<WatchlistDTO>>.Success(watchlistDtos);
        }
        public async Task<Result<List<WatchlistDTO>>> GetAuctionWatchersAsync(int auctionId)
        {
            if (await _uow.Auctions.GetAuctionByIdAsync(auctionId) is null)
                return Result<List<WatchlistDTO>>.Failure("Auction not found.");

            var watchlists = await _uow.Watchlist.GetAuctionWatchersAsync(auctionId);

            var dtos = watchlists.Select(w => new WatchlistDTO
            {
                WatchlistId = w.WatchlistId,
                UserId = w.UserId,
                AuctionId = w.AuctionId,
                CreatedUtc = w.CreatedUtc
            }).ToList();

            return Result<List<WatchlistDTO>>.Success(dtos);
        }
        public async Task<Result<int?>> GetHighestBidderIdAsync(int auctionId)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            if (auction == null)
                return Result<int?>.Failure("Auction not found");

            var highestBidderId = await _uow.Bids.GetHighestBidderIdAsync(auctionId);
            return Result<int?>.Success(highestBidderId);
        }
        public async Task<Result<DateTime?>> GetOldestAuctionDateAsync()
        {
            try
            {
                var oldestDate = await _uow.Auctions.GetOldestAuctionDateAsync();
                return Result<DateTime?>.Success(oldestDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving oldest auction date.");
                return Result<DateTime?>.Failure("Failed to retrieve oldest auction date.");
            }
        }
    }
}