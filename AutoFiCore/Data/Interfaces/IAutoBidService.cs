using AutoFiCore.Dto;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IAutoBidService
    {
        Task<Result<CreateAutoBidDTO>> CreateAutoBidAsync(CreateAutoBidDTO dto);
        Task<Result<string>> UpdateAutoBidAsync(int auctionId, int userId, UpdateAutoBidDTO dto);
        Task<Result<string>> CancelAutoBidAsync(int auctionId, int userId);
        Task<Result<AutoBidSummaryDto>> GetAuctionAutoBidSummaryAsync(int auctionId);
        Task<Result<string>> ProcessAutoBidTrigger(int auctionId, decimal newBidAmount);
        Task<Result<bool>> IsAutoBidSetAsync(int auctionId, int userId);
        Task<Result<CreateAutoBidDTO?>> GetAutoBidWithStrategyAsync(int userId, int auctionId);
    }
}
