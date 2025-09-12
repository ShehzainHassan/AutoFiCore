using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IAuctionService
    {
        Task<Result<AuctionDTO>> CreateAuctionAsync(CreateAuctionDTO dto);
        Task<Result<AuctionDTO?>> UpdateAuctionStatusAsync(int auctionId, AuctionStatus status);
        Task<List<AuctionDTO>> GetAuctionsAsync(AuctionQueryParams filters);
        Task<Result<AuctionDTO?>> GetAuctionByIdAsync(int id);
        Task<Result<BidDTO>> PlaceBidAsync(int auctionId, CreateBidDTO dto);
        Task<Result<List<BidDTO>>> GetBidHistoryAsync(int auctionId);
        Task<Result<string>> AddToWatchListAsync(int userId, int auctionId);
        Task<Result<string>> RemoveFromWatchListAsync(int userId, int auctionId);
        Task<Result<List<WatchlistDTO>>> GetUserWatchListAsync(int userId);
        Task<Result<List<BidDTO>>> GetUserBidHistoryAsync(int userId);
        Task<Result<List<WatchlistDTO>>> GetAuctionWatchersAsync(int auctionId);
        Task<Result<int?>> GetHighestBidderIdAsync(int auctionId);
        Task<Result<AuctionResultDTO?>> ProcessAuctionResultAsync(int auctionId);
        Task<DateTime?> GetOldestAuctionDateAsync();
    }
}
