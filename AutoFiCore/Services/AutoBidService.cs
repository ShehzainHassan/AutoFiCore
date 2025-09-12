using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Mappers;
using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Services
{
    public interface IAutoBidService
    {
        Task<Result<CreateAutoBidDTO>> CreateAutoBidAsync(CreateAutoBidDTO dto);
        Task<Result<string>> UpdateAutoBidAsync(int auctionId, int userId, UpdateAutoBidDTO dto);
        Task<Result<string>> CancelAutoBidAsync(int auctionId, int userId);
        Task<AutoBidSummaryDto?> GetAuctionAutoBidSummaryAsync(int auctionId);
        Task ProcessAutoBidTrigger(int auctionId, decimal newBidAmount);
        Task<Result<bool>> IsAutoBidSetAsync(int auctionId, int userId);
        Task<Result<CreateAutoBidDTO?>> GetAutoBidWithStrategyAsync(int userId, int auctionId);
    }
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
        public async Task ProcessAutoBidTrigger(int auctionId, decimal newBidAmount)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            if (auction == null)
            {
                _log.LogWarning("Auction with {AuctionId} not found", auctionId);
                return;
            }

            if (auction.Status != AuctionStatus.Active)
            {
                _log.LogWarning("Auction is not active");
                return;
            }

            var auctionsWithAutoBids = await _uow.Auctions.GetAuctionsWithActiveAutoBidsAsync();

            _log.LogInformation("Found {Count} auctions with active autobids ", auctionsWithAutoBids.Count);

            var bidCount = (await _uow.Bids.GetBidsByAuctionIdAsync(auctionId)).Count;
            var autoBids = await _uow.AutoBid.GetActiveAutoBidsByAuctionIdAsync(auctionId);
            var sortedAutoBids = autoBids.OrderBy(ab => ab.CreatedAt).ToList();
            var highestBidder = await _uow.Bids.GetHighestBidderIdAsync(auctionId);
            var timeLeft = auction.EndUtc - DateTime.UtcNow;

            foreach (var autoBid in sortedAutoBids)
            {
                try
                {
                    if (autoBid.UserId == highestBidder)
                    {
                        _log.LogInformation("User {UserId} is already highest bidder on auction {AuctionId}. Skipping auto-bid", autoBid.UserId, autoBid.AuctionId);
                        continue;
                    }
                    var bidStrategy = await _uow.AutoBid.GetBidStrategyByUserAndAuctionAsync(autoBid.UserId, auctionId);
                    var increment = BidIncrementCalculator.GetIncrementByStrategy(newBidAmount, bidCount, autoBid.BidStrategyType);
                    var nextBidAmount = newBidAmount + increment;

                    if (bidStrategy == null)
                    {
                        _log.LogWarning("No bid strategy found for user {UserId} on auction {AuctionId}", autoBid.UserId, auctionId);
                        continue;
                    }
                    if (nextBidAmount > autoBid.MaxBidAmount)
                    {
                        _log.LogInformation("Next bid {Amount} exceeds user {UserId}'s max bid", nextBidAmount, autoBid.UserId);
                        continue;
                    }

                    switch (bidStrategy.PreferredBidTiming)
                    {
                        case PreferredBidTiming.LastMinute:
                            if (timeLeft.TotalSeconds <= 120 && await CanPlaceLastMinuteBid(autoBid, auctionId))
                            {
                                await PlaceAutoBid(autoBid, auctionId, nextBidAmount);
                                return;
                            }
                            break;

                        case PreferredBidTiming.Immediate:
                            if (await CanPlaceImmediateBid(autoBid, auctionId))
                            {
                                await PlaceAutoBid(autoBid, auctionId, nextBidAmount);
                                return;
                            }
                            break;

                        case PreferredBidTiming.SpreadEvenly:
                            if (await CanPlaceSpreadBid(autoBid, auctionId))
                            {
                                await PlaceAutoBid(autoBid, auctionId, nextBidAmount);
                                return;
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Exception while processing auto-bid for user {UserId} on auction {AuctionId}", autoBid.UserId, auctionId);
                }
            }
        }
        public async Task<Result<CreateAutoBidDTO>> CreateAutoBidAsync(CreateAutoBidDTO dto)
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

            _log.LogInformation("Auto-bid created for auction {Auction}", autoBid.AuctionId);

            return Result<CreateAutoBidDTO>.Success(dto);
        }
        public async Task<Result<string>> CancelAutoBidAsync(int auctionId, int userId)
        {
            var autoBid = await _uow.AutoBid.GetByIdAsync(userId, auctionId);

            if (autoBid == null)
                return Result<string>.Failure("Auto-bid not found");

            if (!autoBid.IsActive)
                return Result<string>.Failure("Auto-bid is already inactive");

            await _uow.AutoBid.SetInactiveAsync(userId, auctionId);
            await _uow.SaveChangesAsync();

            _log.LogInformation("Auto-bid and bid strategy removed for user {UserId} on auction {AuctionId}", userId, auctionId);

            return Result<string>.Success("Auto-bid cancelled and bid strategy deleted successfully");
        }
        public async Task<Result<string>> UpdateAutoBidAsync(int auctionId, int userId, UpdateAutoBidDTO dto)
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

            if (dto.BidStrategyType.HasValue &&
                Enum.IsDefined(typeof(BidStrategyType), dto.BidStrategyType.Value))
            {
                autoBid.BidStrategyType = (BidStrategyType)dto.BidStrategyType.Value;
            }

            else if (dto.BidStrategyType.HasValue)
            {
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

            _log.LogInformation("Auto-bid updated for (auction {Auction})", autoBid.AuctionId);
            return Result<string>.Success("Auto-bid updated successfully");
        }
        public async Task<AutoBidSummaryDto?> GetAuctionAutoBidSummaryAsync(int auctionId)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            if (auction == null)
                return null;

            var autoBids = await _uow.AutoBid.GetActiveAutoBidsByAuctionIdAsync(auctionId);

            if (autoBids == null || !autoBids.Any())
            {
                return new AutoBidSummaryDto
                {
                    AuctionId = auctionId,
                    ActiveAutoBidCount = 0,
                    AverageMaxAmount = 0
                };
            }

            var summary = new AutoBidSummaryDto
            {
                AuctionId = auctionId,
                ActiveAutoBidCount = autoBids.Count,
                AverageMaxAmount = autoBids.Average(ab => ab.MaxBidAmount)
            };

            return summary;
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