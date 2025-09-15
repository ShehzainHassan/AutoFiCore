using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Mappers;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Services
{
    public class AutoBidService : IAutoBidService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AutoBidService> _log;
        private readonly IAuctionService _auctionService;
        private readonly IAuctionLifecycleService _auctionLifecycleService;
        public AutoBidService(IUnitOfWork uow, ILogger<AutoBidService> log, IAuctionService auctionService, IAuctionLifecycleService auctionLifecycleService )
        {
            _uow = uow;
            _log = log;
            _auctionService = auctionService;
            _auctionLifecycleService = auctionLifecycleService;
        }

        private async Task<bool> CanPlaceImmediateBid(AutoBid autoBid, int auctionId)
        {
            var bidStrategy = await _uow.AutoBid.GetBidStrategyByUserAndAuctionAsync(autoBid.UserId, auctionId);
            if (bidStrategy == null)
                return false;

            var now = DateTime.UtcNow;

            var userBids = await _uow.Bids.GetBidsByUserIdAsync(autoBid.UserId);

            var auctionBids = userBids
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.CreatedUtc)
                .ToList();

            if (!auctionBids.Any()) return true;

            var lastBid = auctionBids.First();

            var timeSinceLastBid = (now - lastBid.CreatedUtc).TotalSeconds;

            if (timeSinceLastBid < bidStrategy.BidDelaySeconds)
                return false;

            var oneMinuteAgo = now.AddMinutes(-1);
            var bidsInLastMinute = auctionBids.Count(b => b.CreatedUtc > oneMinuteAgo);

            return bidsInLastMinute < bidStrategy.MaxBidsPerMinute;
        }
        private async Task PlaceAutoBid(AutoBid autoBid, int auctionId, decimal amount)
        {
            var result = await _auctionService.PlaceBidAsync(auctionId, new CreateBidDTO
            {
                Amount = amount,
                UserId = autoBid.UserId,
                IsAuto = true
            });

            if (result.IsSuccess)
            {
                _log.LogInformation("Auto-bid placed by user {UserId} on auction {AuctionId} for {Amount}", autoBid.UserId, auctionId, amount);
                await _auctionLifecycleService.HandleAutoBidAsync(auctionId, autoBid.UserId, amount); 
            }
            else
            {
                _log.LogWarning("Auto-bid failed for user {UserId} on auction {AuctionId}: {Reason}", autoBid.UserId, auctionId, result.Error);
            }
        }
        private async Task<bool> CanPlaceSpreadBid(AutoBid autoBid, int auctionId)
        {
            var bidStrategy = await _uow.AutoBid.GetBidStrategyByUserAndAuctionAsync(autoBid.UserId, auctionId);
            if (bidStrategy == null)
            {
                _log.LogWarning("No bid strategy found for user {UserId} on auction {AuctionId}", autoBid.UserId, auctionId);
                return false;
            }

            var allUserBids = await _uow.Bids.GetBidsByUserIdAsync(autoBid.UserId);
            var auctionUserAutoBids = allUserBids
                .Where(b => b.AuctionId == auctionId && b.IsAuto)
                .OrderByDescending(b => b.CreatedUtc)
                .ToList();

            if (auctionUserAutoBids.Count >= bidStrategy.MaxSpreadBids)
            {
                _log.LogInformation("User {UserId} reached MaxSpreadBids ({MaxBids}) for auction {AuctionId}",
                    autoBid.UserId, bidStrategy.MaxSpreadBids, auctionId);
                return false;
            }

            if (!auctionUserAutoBids.Any())
                return true;

            var allAuctionBids = await _uow.Bids.GetBidsByAuctionIdAsync(auctionId);
            if (allAuctionBids == null || !allAuctionBids.Any())
            {
                _log.LogInformation("No bids found on auction {AuctionId}", auctionId);
                return false;
            }

            var bids = await _uow.Bids.GetBidsByAuctionIdAsync(auctionId);
            var latestBid = bids.OrderByDescending(b => b.CreatedUtc).FirstOrDefault();

            if (latestBid == null)
            {
                _log.LogInformation("No bids found for auction {AuctionId}", auctionId);
                return false;
            }

            var now = DateTime.UtcNow;

            var requiredNextBidTime = latestBid.CreatedUtc.AddMinutes(10);

            _log.LogInformation("User {UserId}'s next spread bid is at {NextBidTime}",
                autoBid.UserId, requiredNextBidTime.ToString("HH:mm:ss"));

            if (now >= requiredNextBidTime)
            {
                return true;
            }

            return false;

        }
        private async Task<bool> CanPlaceLastMinuteBid(AutoBid autoBid, int auctionId)
        {
            var userBids = await _uow.Bids.GetBidsByUserIdAsync(autoBid.UserId);

            var lastMinuteBids = userBids
                .Where(b => b.AuctionId == auctionId && b.IsAuto && b.PreferredBidTiming == PreferredBidTiming.LastMinute)
                .OrderByDescending(b => b.CreatedUtc)
                .ToList();

            if (!lastMinuteBids.Any())
                return true;

            _log.LogInformation("User {UserId} already placed a last-minute bid for auction {AuctionId}.", autoBid.UserId, auctionId);
            return false;
        }
        private BidStrategy CreateBidStrategyFromDto(CreateAutoBidDTO dto, DateTime now)
        {
            return new BidStrategy
            {
                UserId = dto.UserId,
                AuctionId = dto.AuctionId,
                Type = dto.BidStrategyType,
                PreferredBidTiming = dto.PreferredBidTiming,
                BidDelaySeconds = dto.PreferredBidTiming == PreferredBidTiming.LastMinute ? null : dto.BidDelaySeconds,
                MaxBidsPerMinute = dto.PreferredBidTiming == PreferredBidTiming.LastMinute ? null : dto.MaxBidsPerMinute,
                MaxSpreadBids = dto.PreferredBidTiming == PreferredBidTiming.SpreadEvenly ? dto.MaxSpreadBids : null,
                CreatedAt = now,
                UpdatedAt = now
            };
        }
        private void UpdateBidStrategyFromDto(BidStrategy strategy, CreateAutoBidDTO dto, DateTime now)
        {
            strategy.Type = dto.BidStrategyType;
            strategy.PreferredBidTiming = dto.PreferredBidTiming;
            strategy.BidDelaySeconds = dto.PreferredBidTiming == PreferredBidTiming.LastMinute ? null : dto.BidDelaySeconds;
            strategy.MaxBidsPerMinute = dto.PreferredBidTiming == PreferredBidTiming.LastMinute ? null : dto.MaxBidsPerMinute;
            strategy.MaxSpreadBids = dto.PreferredBidTiming == PreferredBidTiming.SpreadEvenly ? dto.MaxSpreadBids : null;
            strategy.UpdatedAt = now;
        }
        public async Task<Result<string>> ProcessAutoBidTrigger(int auctionId, decimal newBidAmount)
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
                            _log.LogWarning("Auction with {AuctionId} not found", auctionId);
                            return Result<string>.Failure("Auction not found.");
                        }

                        if (auction.Status != AuctionStatus.Active)
                        {
                            _log.LogWarning("Auction {AuctionId} is not active", auctionId);
                            return Result<string>.Failure("Auction is not active.");
                        }

                        var bidCount = (await _uow.Bids.GetBidsByAuctionIdAsync(auctionId)).Count;
                        var autoBids = await _uow.AutoBid.GetActiveAutoBidsByAuctionIdAsync(auctionId);
                        var sortedAutoBids = autoBids.OrderBy(ab => ab.CreatedAt).ToList();
                        var highestBidder = await _uow.Bids.GetHighestBidderIdAsync(auctionId);
                        var timeLeft = auction.EndUtc - DateTime.UtcNow;

                        foreach (var autoBid in sortedAutoBids)
                        {
                            if (autoBid.UserId == highestBidder)
                            {
                                _log.LogInformation("User {UserId} is already highest bidder on auction {AuctionId}. Skipping auto-bid", autoBid.UserId, auctionId);
                                continue;
                            }

                            var bidStrategy = await _uow.AutoBid.GetBidStrategyByUserAndAuctionAsync(autoBid.UserId, auctionId);
                            if (bidStrategy == null)
                            {
                                _log.LogWarning("No bid strategy found for user {UserId} on auction {AuctionId}", autoBid.UserId, auctionId);
                                continue;
                            }

                            var increment = BidIncrementCalculator.GetIncrementByStrategy(newBidAmount, bidCount, autoBid.BidStrategyType);
                            var nextBidAmount = newBidAmount + increment;

                            if (nextBidAmount > autoBid.MaxBidAmount)
                            {
                                _log.LogInformation("Next bid {Amount} exceeds user {UserId}'s max bid", nextBidAmount, autoBid.UserId);
                                continue;
                            }

                            bool bidPlaced = false;

                            switch (bidStrategy.PreferredBidTiming)
                            {
                                case PreferredBidTiming.LastMinute:
                                    if (timeLeft.TotalSeconds <= 120 && await CanPlaceLastMinuteBid(autoBid, auctionId))
                                    {
                                        await PlaceAutoBid(autoBid, auctionId, nextBidAmount);
                                        bidPlaced = true;
                                    }
                                    break;

                                case PreferredBidTiming.Immediate:
                                    if (await CanPlaceImmediateBid(autoBid, auctionId))
                                    {
                                        await PlaceAutoBid(autoBid, auctionId, nextBidAmount);
                                        bidPlaced = true;
                                    }
                                    break;

                                case PreferredBidTiming.SpreadEvenly:
                                    if (await CanPlaceSpreadBid(autoBid, auctionId))
                                    {
                                        await PlaceAutoBid(autoBid, auctionId, nextBidAmount);
                                        bidPlaced = true;
                                    }
                                    break;
                            }

                            if (bidPlaced)
                            {
                                await _uow.SaveChangesAsync();
                                await _uow.CommitTransactionAsync();
                                _log.LogInformation("Auto-bid placed by user {UserId} on auction {AuctionId}", autoBid.UserId, auctionId);
                                return Result<string>.Success($"Auto-bid placed by user {autoBid.UserId}.");
                            }
                        }

                        await _uow.CommitTransactionAsync();
                        return Result<string>.Failure("No eligible auto-bid placed.");
                    }
                    catch (Exception ex)
                    {
                        await _uow.RollbackTransactionAsync();
                        _log.LogError(ex, "Exception while processing auto-bid trigger for auction {AuctionId}", auctionId);
                        return Result<string>.Failure("Unexpected error occurred during auto-bid trigger.");
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Execution strategy failed during auto-bid trigger for auction {AuctionId}", auctionId);
                return Result<string>.Failure("Execution strategy failed.");
            }
        }
        public async Task<Result<CreateAutoBidDTO>> CreateAutoBidAsync(CreateAutoBidDTO dto)
        {
            var strategy = _uow.DbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await _uow.BeginTransactionAsync();
                    try
                    {
                        var auction = await _uow.Auctions.GetAuctionByIdAsync(dto.AuctionId);
                        if (auction == null || auction.Status != AuctionStatus.Active)
                            return Result<CreateAutoBidDTO>.Failure("Auction not found or not active.");

                        decimal highestBid = await _uow.Bids.GetHighestBidAmountAsync(auction.AuctionId, auction.StartingPrice);
                        int totalBids = (await _uow.Bids.GetBidsByAuctionIdAsync(auction.AuctionId)).Count;
                        var minIncrement = BidIncrementCalculator.GetIncrementByStrategy(highestBid, totalBids, dto.BidStrategyType);

                        if (dto.MaxBidAmount < auction.StartingPrice + minIncrement)
                            return Result<CreateAutoBidDTO>.Failure(
                                $"MaxBidAmount must be at least {(auction.StartingPrice + minIncrement):C} based on the selected strategy.");

                        var existingAutoBid = await _uow.AutoBid.GetAutoBidWithStrategyAsync(dto.UserId, dto.AuctionId);
                        var now = DateTime.UtcNow;

                        if (existingAutoBid != null)
                        {
                            var updateDto = new UpdateAutoBidDTO
                            {
                                MaxBidAmount = dto.MaxBidAmount,
                                BidStrategyType = (int?)dto.BidStrategyType,
                                IsActive = dto.IsActive
                            };

                            var updateResult = await UpdateAutoBidAsync(dto.AuctionId, dto.UserId, updateDto);

                            var strategy = await _uow.AutoBid.GetBidStrategyByUserAndAuctionAsync(dto.UserId, dto.AuctionId);
                            if (strategy != null)
                            {
                                UpdateBidStrategyFromDto(strategy, dto, now);
                            }

                            await _uow.SaveChangesAsync();
                            await _uow.CommitTransactionAsync();

                            return updateResult.IsSuccess
                                ? Result<CreateAutoBidDTO>.Success(dto)
                                : Result<CreateAutoBidDTO>.Failure(updateResult.Error ?? "Failed to update AutoBid.");
                        }

                        var autoBid = new AutoBid
                        {
                            UserId = dto.UserId,
                            AuctionId = dto.AuctionId,
                            MaxBidAmount = dto.MaxBidAmount,
                            CurrentBidAmount = 0,
                            BidStrategyType = dto.BidStrategyType,
                            IsActive = dto.IsActive,
                            CreatedAt = now,
                            UpdatedAt = now
                        };

                        var strategyToAdd = CreateBidStrategyFromDto(dto, now);

                        await _uow.AutoBid.AddAutoBidAsync(autoBid);
                        await _uow.AutoBid.AddBidStrategyAsync(strategyToAdd);
                        await _uow.SaveChangesAsync();
                        await _uow.CommitTransactionAsync();

                        _log.LogInformation("Auto-bid created for auction {Auction}", autoBid.AuctionId);
                        return Result<CreateAutoBidDTO>.Success(dto);
                    }
                    catch (Exception ex)
                    {
                        await _uow.RollbackTransactionAsync();
                        _log.LogError(ex, "Failed to create/update AutoBid for auction {AuctionId} and user {UserId}", dto.AuctionId, dto.UserId);
                        return Result<CreateAutoBidDTO>.Failure("Unexpected error occurred while creating AutoBid.");
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Execution strategy failed for AutoBid creation on auction {AuctionId}", dto.AuctionId);
                return Result<CreateAutoBidDTO>.Failure("Unexpected error occurred during AutoBid creation.");
            }
        }
        public async Task<Result<string>> CancelAutoBidAsync(int auctionId, int userId)
        {
            var strategy = _uow.DbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await _uow.BeginTransactionAsync();
                    try
                    {
                        var autoBid = await _uow.AutoBid.GetByIdAsync(userId, auctionId);

                        if (autoBid == null)
                            return Result<string>.Failure("Auto-bid not found");

                        if (!autoBid.IsActive)
                            return Result<string>.Failure("Auto-bid is already inactive");

                        await _uow.AutoBid.SetInactiveAsync(userId, auctionId);
                        await _uow.SaveChangesAsync();
                        await _uow.CommitTransactionAsync();

                        _log.LogInformation("Auto-bid and bid strategy removed for user {UserId} on auction {AuctionId}", userId, auctionId);
                        return Result<string>.Success("Auto-bid cancelled and bid strategy deleted successfully");
                    }
                    catch (Exception ex)
                    {
                        await _uow.RollbackTransactionAsync();
                        _log.LogError(ex, "Failed to cancel AutoBid for user {UserId} on auction {AuctionId}", userId, auctionId);
                        return Result<string>.Failure("Unexpected error occurred while cancelling AutoBid.");
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Execution strategy failed while cancelling AutoBid for auction {AuctionId}", auctionId);
                return Result<string>.Failure("Unexpected error occurred during AutoBid cancellation.");
            }
        }
        public async Task<Result<string>> UpdateAutoBidAsync(int auctionId, int userId, UpdateAutoBidDTO dto)
        {
            var strategy = _uow.DbContext.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await _uow.BeginTransactionAsync();
                    try
                    {
                        var autoBid = await _uow.AutoBid.GetByIdAsync(userId, auctionId);
                        if (autoBid == null)
                            return Result<string>.Failure("Auto-bid not found");

                        if (autoBid.UserId != userId)
                            return Result<string>.Failure("User Id does not match. Access denied.");

                        var auction = await _uow.Auctions.GetAuctionByIdAsync(autoBid.AuctionId);
                        if (auction == null)
                            return Result<string>.Failure("Auction not found");

                        if (auction.Status != AuctionStatus.Active)
                            return Result<string>.Failure("Auction is not active.");

                        if (dto.MaxBidAmount < auction.CurrentPrice)
                            return Result<string>.Failure("MaxBidAmount cannot be less than current bid.");

                        if (dto.MaxBidAmount.HasValue)
                            autoBid.MaxBidAmount = dto.MaxBidAmount.Value;

                        if (dto.BidStrategyType.HasValue)
                        {
                            if (Enum.IsDefined(typeof(BidStrategyType), dto.BidStrategyType.Value))
                                autoBid.BidStrategyType = (BidStrategyType)dto.BidStrategyType.Value;
                            else
                                return Result<string>.Failure("Invalid bid strategy type.");
                        }

                        if (dto.IsActive.HasValue)
                            autoBid.IsActive = dto.IsActive.Value;

                        autoBid.UpdatedAt = DateTime.UtcNow;

                        var bidStrategy = await _uow.AutoBid.GetBidStrategyByUserAndAuctionAsync(userId, auctionId);
                        if (bidStrategy != null)
                        {
                            if (dto.BidStrategyType.HasValue &&
                                Enum.IsDefined(typeof(BidStrategyType), dto.BidStrategyType.Value))
                            {
                                bidStrategy.Type = (BidStrategyType)dto.BidStrategyType.Value;
                            }

                            bidStrategy.UpdatedAt = DateTime.UtcNow;
                        }

                        await _uow.SaveChangesAsync();
                        await _uow.CommitTransactionAsync();

                        _log.LogInformation("Auto-bid updated for auction {Auction}", autoBid.AuctionId);
                        return Result<string>.Success("Auto-bid updated successfully");
                    }
                    catch (Exception ex)
                    {
                        await _uow.RollbackTransactionAsync();
                        _log.LogError(ex, "Failed to update AutoBid for user {UserId} on auction {AuctionId}", userId, auctionId);
                        return Result<string>.Failure("Unexpected error occurred while updating AutoBid.");
                    }
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Execution strategy failed while updating AutoBid for auction {AuctionId}", auctionId);
                return Result<string>.Failure("Unexpected error occurred during AutoBid update.");
            }
        }
        public async Task<Result<AutoBidSummaryDto>> GetAuctionAutoBidSummaryAsync(int auctionId)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            if (auction == null)
                return Result<AutoBidSummaryDto>.Failure("Auction not found.");

            var autoBids = await _uow.AutoBid.GetActiveAutoBidsByAuctionIdAsync(auctionId);

            var summary = new AutoBidSummaryDto
            {
                AuctionId = auctionId,
                ActiveAutoBidCount = autoBids?.Count ?? 0,
                AverageMaxAmount = (autoBids != null && autoBids.Any())
                    ? autoBids.Average(ab => ab.MaxBidAmount)
                    : 0
            };

            return Result<AutoBidSummaryDto>.Success(summary);
        }
        public async Task<Result<bool>> IsAutoBidSetAsync(int auctionId, int userId)
        {
            var user = await _uow.Users.GetUserByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            if (auction == null)
                return Result<bool>.Failure("Auction not found");

            bool isActive = await _uow.AutoBid.IsActiveAsync(userId, auctionId);
            return Result<bool>.Success(isActive);
        }
        public async Task<Result<CreateAutoBidDTO?>> GetAutoBidWithStrategyAsync(int userId, int auctionId)
        {
            var user = await _uow.Users.GetUserByIdAsync(userId);
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);

            if (user == null)
                return Result<CreateAutoBidDTO?>.Failure("User not found");

            if (auction == null)
                return Result<CreateAutoBidDTO?>.Failure("Auction not found");

            var result = await _uow.AutoBid.GetAutoBidWithStrategyAsync(userId, auctionId);
            return Result<CreateAutoBidDTO?>.Success(result);
        }
    }
}