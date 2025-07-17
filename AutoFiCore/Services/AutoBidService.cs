using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Mappers;
using AutoFiCore.Utilities;

namespace AutoFiCore.Services
{
    public interface IAutoBidService
    {
        Task<Result<AutoBidDTO>> CreateAutoBidAsync(CreateAutoBidDTO dto, int userId);
        Task<Result<AutoBidDTO>> UpdateAutoBidAsync(int id, UpdateAutoBidDTO dto, int userId);
        Task<Result<AutoBidDTO>> CancelAutoBidAsync(int id, int userId);
        Task<List<AutoBidDTO>> GetActiveAutoBidsForUserAsync(int userId);
        Task<AutoBidSummaryDto?> GetAuctionAutoBidSummaryAsync(int auctionId);
    }
    public class AutoBidService:IAutoBidService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AutoBidService> _log;

        public AutoBidService(IUnitOfWork uow, ILogger<AutoBidService> log)
        {
            _uow = uow;
            _log = log;
        }

        public async Task<Result<AutoBidDTO>> CreateAutoBidAsync(CreateAutoBidDTO dto, int userId)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(dto.AuctionId);
            if (auction == null || auction.Status != AuctionStatus.Active)
                return Result<AutoBidDTO>.Failure("Auction not found or not active.");

            if (dto.MaxBidAmount <= auction.CurrentPrice)
                return Result<AutoBidDTO>.Failure("MaxBidAmount must be greater than current price.");

            if (await _uow.AutoBid.IsActiveAsync(userId, dto.AuctionId))
                return Result<AutoBidDTO>.Failure("An active auto-bid already exists for this auction.");

            var ab = new AutoBid
            {
                UserId = userId,
                AuctionId = dto.AuctionId,
                MaxBidAmount = dto.MaxBidAmount,
                BidStrategyType = dto.BidStrategyType,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _uow.AutoBid.AddAutoBidAsync(ab);
            await _uow.SaveChangesAsync();

            _log.LogInformation("Auto-bid created for (auction {Auction})",ab.AuctionId);

            return Result<AutoBidDTO>.Success(AutoBidMapper.ToDTO(ab));
        }
        public async Task<Result<AutoBidDTO>> CancelAutoBidAsync(int id, int userId)
        {
            var autoBid = await _uow.AutoBid.GetByIdAsync(id);

            if (autoBid == null)
                return Result<AutoBidDTO>.Failure("Auto-bid not found");

            if (autoBid.UserId != userId)
                return Result<AutoBidDTO>.Failure("User Id does not match. Access denied.");

            if (!autoBid.IsActive)
                return Result<AutoBidDTO>.Failure("Auto bid is already inactive");

            await _uow.AutoBid.SetInactiveAsync(id);
          
            await _uow.SaveChangesAsync();

            _log.LogInformation("Auto-bid cancelled for (auction {Auction})", autoBid.AuctionId);
            return Result<AutoBidDTO>.Success(AutoBidMapper.ToDTO(autoBid));

        }
        public async Task<Result<AutoBidDTO>> UpdateAutoBidAsync(int id, UpdateAutoBidDTO dto, int userId)
        {
            var autoBid = await _uow.AutoBid.GetByIdAsync(id);
            
            if (autoBid == null)
                return Result<AutoBidDTO>.Failure("Auto-bid not found");

            if (autoBid.UserId != userId)
                return Result<AutoBidDTO>.Failure("User Id does not match. Access denied.");

            var auction = await _uow.Auctions.GetAuctionByIdAsync(autoBid.AuctionId);
            if (auction == null)
                return Result<AutoBidDTO>.Failure("Auction not found");

            if (auction.Status != AuctionStatus.Active)
                return Result<AutoBidDTO>.Failure("Auction is not active.");

            if (dto.MaxBidAmount < auction.CurrentPrice)
                return Result<AutoBidDTO>.Failure("MaxBidAmount cannot be less than current bid.");

            if (dto.MaxBidAmount.HasValue)
                autoBid.MaxBidAmount = dto.MaxBidAmount.Value;

            if (dto.BidStrategyType.HasValue &&
                Enum.IsDefined(typeof(BidStrategyType), dto.BidStrategyType.Value))
            {
                autoBid.BidStrategyType = (BidStrategyType)dto.BidStrategyType.Value;
            }
            else if (dto.BidStrategyType.HasValue)
            {
                return Result<AutoBidDTO>.Failure("Invalid bid strategy type.");
            }
            if (dto.IsActive.HasValue)
                autoBid.IsActive = dto.IsActive.Value;
            autoBid.UpdatedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync();

            _log.LogInformation("Auto-bid updated for (auction {Auction})", autoBid.AuctionId);
            return Result<AutoBidDTO>.Success(AutoBidMapper.ToDTO(autoBid));
        }
        public async Task<List<AutoBidDTO>> GetActiveAutoBidsForUserAsync(int userId)
        {
            var autoBids = await _uow.AutoBid.GetActiveAutoBidsByUserAsync(userId);

            return autoBids.Select(AutoBidMapper.ToDTO).ToList();
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

    }
}
