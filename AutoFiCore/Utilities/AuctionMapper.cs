using AutoFiCore.Dto;
using AutoFiCore.Models;

namespace AutoFiCore.Mappers
{
    /// <summary>
    /// Provides mapping logic between <see cref="Auction"/> domain models and <see cref="AuctionDTO"/> data transfer objects.
    /// </summary>
    public static class AuctionMapper
    {
        /// <summary>
        /// Converts an <see cref="Auction"/> entity into an <see cref="AuctionDTO"/>, including nested vehicle and bid data.
        /// </summary>
        /// <param name="auction">The auction domain model to convert.</param>
        /// <returns>A DTO representation of the auction.</returns>
        public static AuctionDTO ToDTO(Auction auction)
        {
            return new AuctionDTO
            {
                AuctionId = auction.AuctionId,
                StartingPrice = auction.StartingPrice,
                ReservePrice = auction.ReservePrice,
                CurrentPrice = auction.CurrentPrice,
                Status = auction.Status,
                StartUtc = auction.StartUtc,
                EndUtc = auction.EndUtc,
                PreviewStartTime = auction.PreviewStartTime,
                ScheduledStartTime = auction.ScheduledStartTime,
                UpdatedUtc = auction.UpdatedUtc,
                Vehicle = auction.Vehicle == null ? null : new VehicleDTO
                {
                    Id = auction.Vehicle.Id,
                    Vin = auction.Vehicle.Vin,
                    Make = auction.Vehicle.Make,
                    Model = auction.Vehicle.Model,
                    Year = auction.Vehicle.Year,
                    Price = auction.Vehicle.Price,
                    Mileage = auction.Vehicle.Mileage,
                    Color = auction.Vehicle.Color!,
                    FuelType = auction.Vehicle.FuelType!,
                    Transmission = auction.Vehicle.Transmission!,
                    Status = auction.Vehicle.Status!
                },
                Bids = auction.Bids?
                    .OrderByDescending(b => b.CreatedUtc)
                    .Select(b => new BidDTO
                    {
                        BidId = b.BidId,
                        AuctionId = b.AuctionId,
                        UserId = b.UserId,
                        Amount = b.Amount,
                        IsAuto = b.IsAuto,
                        PlacedAt = b.CreatedUtc,
                    }).ToList()
            };
        }
    }
}