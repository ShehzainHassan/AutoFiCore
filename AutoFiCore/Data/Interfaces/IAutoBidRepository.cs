using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace AutoFiCore.Data.Interfaces
{
    public interface IAutoBidRepository
    {
        Task<AutoBid> AddAutoBidAsync(AutoBid autoBid);
        Task<bool> IsActiveAsync(int userId, int auctionId);
        Task<AutoBid?> GetByIdAsync(int userId, int auctionId);
        Task SetInactiveAsync(int userId, int auctionId);
        Task<BidStrategy?> GetBidStrategyByUserAndAuctionAsync(int userId, int auctionId);
        Task<CreateAutoBidDTO?> GetAutoBidWithStrategyAsync(int userId, int auctionId);
        Task<List<AutoBid>> GetActiveAutoBidsByAuctionIdAsync(int auctionId);
        Task<BidStrategy> AddBidStrategyAsync(BidStrategy bidStrategy);
        Task<List<UserAutoBidSettings>> GetUserAutoBidSettingsAsync(int userId);
        Task SetAllInactiveByAuctionIdAsync(int auctionId);
    }
}
