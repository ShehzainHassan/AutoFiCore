using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Services
{
    public interface IAuctionService
    {
        Task<Result<Auction>> CreateAuctionAsync(CreateAuctionDTO dto);
        Task<Result<Auction>> UpdateAuctionStatusAsync(int auctionId, string status);

    }
    public class AuctionService : IAuctionService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AuctionService> _logger;

        public AuctionService(IUnitOfWork uow, ILogger<AuctionService> log)
        {
            _uow = uow;
            _logger = log;
        }

        public async Task<Result<Auction>> CreateAuctionAsync(CreateAuctionDTO dto)
        {
            var errors = Validator.ValidateAuctionDto(dto);
            if (errors.Any())
                return Result<Auction>.Failure(string.Join("; ", errors));

            var vehicle = await _uow.Vehicles.GetVehicleByIdAsync(dto.VehicleId);
            if (vehicle == null)
                return Result<Auction>.Failure($"Vehicle {dto.VehicleId} not found");

            if (await _uow.Auctions.VehicleHasAuction(dto.VehicleId))
                return Result<Auction>.Failure("An auction already exists for this vehicle");

            var auction = new Auction
            {
                VehicleId = dto.VehicleId,
                StartUtc = dto.StartUtc,
                EndUtc = dto.EndUtc,
                StartingPrice = dto.StartingPrice,
                CurrentPrice = dto.StartingPrice,
                Status = "Active",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            await _uow.Auctions.AddAuctionAsync(auction);

            return Result<Auction>.Success(auction);
        }

        public async Task<Result<Auction>> UpdateAuctionStatusAsync(int auctionId, string status)
        {
            var allowedStatuses = new[] { "Active", "Ended", "Cancelled" }; 
            if (!allowedStatuses.Contains(status))
                return Result<Auction>.Failure("Invalid status.");

            var auction = await _uow.Auctions.UpdateAuctionStatusAsync(auctionId, status);
            if (auction == null)
                return Result<Auction>.Failure("Auction not found.");

            await _uow.SaveChangesAsync();
            return Result<Auction>.Success(auction);
        }

    }
}