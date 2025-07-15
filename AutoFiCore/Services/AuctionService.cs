using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Mappers;
using AutoFiCore.Models;
using AutoFiCore.Queries;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;
using System;

namespace AutoFiCore.Services
{
    public interface IAuctionService
    {
        Task<Result<AuctionDTO>> CreateAuctionAsync(CreateAuctionDTO dto);        
        Task<Result<Auction>> UpdateAuctionStatusAsync(int auctionId, string status);
        Task<List<AuctionDTO>> GetAuctionsAsync(AuctionQueryParams filters);
        Task<Result<AuctionDTO?>> GetAuctionByIdAsync(int id);
        Task<Result<BidDTO>> PlaceBidAsync(int auctionId, CreateBidDTO dto);
        Task<Result<List<BidDTO>>> GetBidHistoryAsync(int auctionId);
        Task<Result<string>> AddToWatchListAsync(int userId, int auctionId);
        Task<Result<string>> RemoveFromWatchListAsync(int userId, int auctionId);
        Task<Result<List<WatchlistDTO>>> GetUserWatchListAsync(int userId);
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

        public async Task<Result<AuctionDTO>> CreateAuctionAsync(CreateAuctionDTO dto)
        {
            var errors = Validator.ValidateAuctionDto(dto);
            if (errors.Any())
                return Result<AuctionDTO>.Failure(string.Join("; ", errors));

            var vehicle = await _uow.Vehicles.GetVehicleByIdAsync(dto.VehicleId);
            if (vehicle == null)
                return Result<AuctionDTO>.Failure($"Vehicle {dto.VehicleId} not found");

            if (await _uow.Auctions.VehicleHasAuction(dto.VehicleId))
                return Result<AuctionDTO>.Failure("An auction already exists for this vehicle");

            var auction = new Auction
            {
                VehicleId = dto.VehicleId,
                StartUtc = dto.StartUtc,
                EndUtc = dto.EndUtc,
                StartingPrice = dto.StartingPrice,
                CurrentPrice = 0,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            var allowedStatuses = new[] { "Active", "Ended", "Cancelled" };
            if (!allowedStatuses.Contains(dto.Status))
                return Result<AuctionDTO>.Failure("Invalid status.");

            var createdAuction = await _uow.Auctions.AddAuctionAsync(auction);
            await _uow.SaveChangesAsync();
            var dtoResult = AuctionMapper.ToDTO(auction);
            return Result<AuctionDTO>.Success(dtoResult);
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

        public async Task<List<AuctionDTO>> GetAuctionsAsync(AuctionQueryParams filters)
        {
            var query = _uow.Auctions.Query().Include(a => a.Vehicle).Include(a => a.Bids);
            var filteredQuery = AuctionQuery.ApplyFilters(query, filters);
            var sortedQuery = AuctionQuery.ApplySorting(filteredQuery, filters);
            var auctions = await sortedQuery.AsNoTracking().ToListAsync();

            return auctions.Select(AuctionMapper.ToDTO).ToList();
        }

        public async Task<Result<AuctionDTO?>> GetAuctionByIdAsync(int id)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(id);

            if (auction == null)
                return Result<AuctionDTO?>.Failure("Auction not found.");

            return Result<AuctionDTO?>.Success(AuctionMapper.ToDTO(auction));
        }

        public async Task<Result<BidDTO>> PlaceBidAsync(int auctionId, CreateBidDTO dto)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            if (auction == null)
                return Result<BidDTO>.Failure("Auction not found.");

            var user = await _uow.Users.GetUserByIdAsync(dto.UserId);
            if (user == null)
                return Result<BidDTO>.Failure("User not found.");

            if (auction.Status != "Active" || auction.EndUtc <= DateTime.UtcNow)
                return Result<BidDTO>.Failure("Auction has ended.");

            var errs = Validator.ValidateBidAmount(dto.Amount, auction.StartingPrice, auction.CurrentPrice);
            if (errs.Any())
                return Result<BidDTO>.Failure(string.Join("; ", errs));

            var bid = new Bid
            {
                AuctionId = auctionId,
                UserId = dto.UserId,
                Amount = dto.Amount,
                IsAuto = dto.IsAuto ?? false,
                CreatedUtc = DateTime.UtcNow
            };

            await _uow.Bids.AddBidAsync(bid);

            await _uow.Auctions.UpdateCurrentPriceAsync(auctionId);

            await _uow.SaveChangesAsync();

            var bidDto = new BidDTO
            {
                BidId = bid.BidId,
                AuctionId = bid.AuctionId,
                UserId = bid.UserId,
                Amount = bid.Amount,
                IsAuto = bid.IsAuto,
                PlacedAt = bid.CreatedUtc
            };

            return Result<BidDTO>.Success(bidDto);
        }


        public async Task<Result<List<BidDTO>>> GetBidHistoryAsync(int auctionId)
        {
            if (await _uow.Auctions.GetAuctionByIdAsync(auctionId) is null)
                return Result<List<BidDTO>>.Failure("Auction not found.");

            var bids = await _uow.Bids.GetBidsByAuctionIdAsync(auctionId);

            var dtos = bids.Select(b => new BidDTO
            {
                BidId = b.BidId,
                AuctionId = b.AuctionId,
                UserId = b.UserId,
                Amount = b.Amount,
                IsAuto = b.IsAuto,
                PlacedAt = b.CreatedUtc
            }).ToList();

            return Result<List<BidDTO>>.Success(dtos);
        }

        public async Task<Result<string>> AddToWatchListAsync(int userId, int auctionId)
        {
            if (await _uow.Auctions.GetAuctionByIdAsync(auctionId) is null)
                return Result<string>.Failure("Auction not found.");

            if (await _uow.Users.GetUserByIdAsync(userId) is null)
                return Result<string>.Failure("User not found.");

            await _uow.Watchlist.AddToWatchlistAsync(userId, auctionId);
            await _uow.SaveChangesAsync();
            return Result<string>.Success("Auction added to watchlist.");
        }

        public async Task<Result<string>> RemoveFromWatchListAsync(int userId, int auctionId)
        {
            if (await _uow.Auctions.GetAuctionByIdAsync(auctionId) is null)
                return Result<string>.Failure("Auction not found.");

            if (await _uow.Users.GetUserByIdAsync(userId) is null)
                return Result<string>.Failure("User not found.");

            await _uow.Watchlist.RemoveFromWatchlistAsync(userId, auctionId);
            await _uow.SaveChangesAsync();
            return Result<string>.Success("Removed auction from watchlist.");
        }

        public async Task<Result<List<WatchlistDTO>>> GetUserWatchListAsync(int userId)
        {
            if (await _uow.Users.GetUserByIdAsync(userId) is null)
                return Result<List<WatchlistDTO>>.Failure("User not found.");

            var watchlists = await _uow.Watchlist.GetUserWatchlistAsync(userId);

            var dtos = watchlists.Select(w => new WatchlistDTO
            {
                WatchlistId = w.WatchlistId,
                UserId = w.UserId,
                AuctionId = w.AuctionId,
                CreatedUtc = w.CreatedUtc
            }).ToList();

            return Result<List<WatchlistDTO>>.Success(dtos);
        }



    }
}