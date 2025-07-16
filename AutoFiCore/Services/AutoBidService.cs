using AutoFiCore.Data;
using AutoFiCore.Enums;
using AutoFiCore.Mappers;
using AutoFiCore.Utilities;

namespace AutoFiCore.Services
{
    public interface IAutoBidService
    {
        Task<Result<AutoBidDTO>> CreateAutoBidAsync(CreateAutoBidDTO dto, int userId);
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

            _log.LogInformation("Auto-bid {Id} created (auction {Auction})", ab.Id, ab.AuctionId);

            return Result<AutoBidDTO>.Success(AutoBidMapper.ToDTO(ab));
        }

    }
}
