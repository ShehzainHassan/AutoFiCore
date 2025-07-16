using AutoFiCore.Dto;
using AutoFiCore.Models;

namespace AutoFiCore.Mappers
{
    public static class AutoBidMapper
    {
        public static AutoBidDTO ToDTO(AutoBid ab)
        {
            return new AutoBidDTO
            {
                Id = ab.Id,
                UserId = ab.UserId,
                AuctionId = ab.AuctionId,
                MaxBidAmount = ab.MaxBidAmount,
                IsActive = ab.IsActive,
                BidStrategyType = ab.BidStrategyType,
                CreatedAt = ab.CreatedAt,
                UpdatedAt = ab.UpdatedAt
            };
        }
    }
}
