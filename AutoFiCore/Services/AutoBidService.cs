using AutoFiCore.Data;
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

        public AutoBidService(IUnitOfWork uow, ILogger<AutoBidService> log, IAuctionService auctionService)
        {
            _uow = uow;
            _log = log;
            _auctionService = auctionService;
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

            var bids = await _uow.Bids.GetBidsByAuctionIdAsync(auctionId);
            var bidCount = bids.Count;

            var auctionAutoBids = await _uow.AutoBid.GetActiveAutoBidsByAuctionIdAsync(auctionId);

            foreach (var autoBid in auctionAutoBids)
            {
                try
                {
                    var highestBidder = await _uow.Bids.GetHighestBidderIdAsync(auctionId);
                    var increment = BidIncrementCalculator.GetIncrementByStrategy(newBidAmount, bidCount, autoBid.BidStrategyType);
                    var nextBidAmount = newBidAmount + increment;

                 
                    _log.LogInformation("Processing auto-bids for auction {AuctionId} with bid {BidAmount}", auctionId, newBidAmount);

                    if (nextBidAmount <= autoBid.MaxBidAmount && autoBid.UserId != highestBidder)
                    {
                        var createBidDto = new CreateBidDTO
                        {
                            Amount = nextBidAmount,
                            UserId = autoBid.UserId,
                            IsAuto = true
                        };

                        var result = await _auctionService.PlaceBidAsync(auctionId, createBidDto);
                        if (result.IsSuccess)
                        {
                            _log.LogInformation("Auto-bid placed by user {UserId} on auction {AuctionId} for {Amount}", autoBid.UserId, auctionId, nextBidAmount);
                            break;
                        }
                        else
                        {
                            _log.LogWarning("Auto-bid failed for user {UserId} on auction {AuctionId}: {Reason}", autoBid.UserId, auctionId, result.Error);
                        }
                    }
                    else
                    {
                        _log.LogWarning("User {UserId} on auction {AuctionId} is already highest bidder. Skipping auto-bid", autoBid.UserId, auctionId);
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

            if (auction.CurrentPrice == 0 && dto.MaxBidAmount < auction.StartingPrice)
                return Result<CreateAutoBidDTO>.Failure("MaxBidAmount must be greater than starting price.");

            if (dto.MaxBidAmount <= auction.CurrentPrice)
                return Result<CreateAutoBidDTO>.Failure("MaxBidAmount must be greater than current price.");

            var existingAutoBid = await _uow.AutoBid.GetAutoBidWithStrategyAsync(dto.UserId, dto.AuctionId);

            if (existingAutoBid != null)
            {
                var updateDto = new UpdateAutoBidDTO
                {
                    MaxBidAmount = dto.MaxBidAmount,
                    BidStrategyType = (int?)dto.BidStrategyType,
                    IsActive = dto.IsActive
                };

                var updateResult = await UpdateAutoBidAsync(dto.AuctionId, dto.UserId, updateDto);

                var existingStrategy = await _uow.AutoBid.GetBidStrategyByUserAndAuctionAsync(dto.UserId, dto.AuctionId);
                if (existingStrategy != null)
                {
                    existingStrategy.Type = dto.BidStrategyType;
                    existingStrategy.PreferredBidTiming = dto.PreferredBidTiming;
                    existingStrategy.BidDelaySeconds = dto.PreferredBidTiming == PreferredBidTiming.LastMinute ? null : dto.BidDelaySeconds;
                    existingStrategy.MaxBidsPerMinute = dto.PreferredBidTiming == PreferredBidTiming.LastMinute ? null : dto.MaxBidsPerMinute;
                    existingStrategy.UpdatedAt = DateTime.UtcNow;
                }

                await _uow.SaveChangesAsync();

                return updateResult.IsSuccess
                    ? Result<CreateAutoBidDTO>.Success(dto)
                    : Result<CreateAutoBidDTO>.Failure(updateResult.Error ?? "Failed to update AutoBid.");
            }

            var ab = new AutoBid
            {
                UserId = dto.UserId,
                AuctionId = dto.AuctionId,
                MaxBidAmount = dto.MaxBidAmount,
                CurrentBidAmount = 0,
                BidStrategyType = dto.BidStrategyType,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var bidStrategy = new BidStrategy
            {
                UserId = dto.UserId,
                AuctionId = dto.AuctionId,
                Type = dto.BidStrategyType,
                BidDelaySeconds = dto.PreferredBidTiming == PreferredBidTiming.LastMinute ? null : dto.BidDelaySeconds,
                MaxBidsPerMinute = dto.PreferredBidTiming == PreferredBidTiming.LastMinute ? null : dto.MaxBidsPerMinute,
                PreferredBidTiming = dto.PreferredBidTiming,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _uow.AutoBid.AddAutoBidAsync(ab);
            await _uow.AutoBid.AddBidStrategyAsync(bidStrategy);
            await _uow.SaveChangesAsync();

            _log.LogInformation("Auto-bid created for auction {Auction}", ab.AuctionId);

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