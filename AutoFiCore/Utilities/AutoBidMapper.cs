using AutoFiCore.Dto;
using AutoFiCore.Models;

namespace AutoFiCore.Mappers
{
    /// <summary>
    /// Provides mapping logic between <see cref="AutoBid"/> domain models and <see cref="AutoBidDTO"/> data transfer objects.
    /// </summary>
    public static class AutoBidMapper
    {
        /// <summary>
        /// Converts an <see cref="AutoBid"/> entity into an <see cref="AutoBidDTO"/>.
        /// </summary>
        /// <param name="ab">The AutoBid domain model.</param>
        /// <returns>A DTO representation of the AutoBid.</returns>
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