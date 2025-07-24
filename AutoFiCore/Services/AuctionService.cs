using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Mappers;
using AutoFiCore.Models;
using AutoFiCore.Queries;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;
using AutoFiCore.Enums;
using System;
using Microsoft.AspNetCore.SignalR;
using AutoFiCore.Hubs;

namespace AutoFiCore.Services
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
    }
    public class AuctionService : IAuctionService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<AuctionService> _logger;
        private readonly IHubContext<AuctionHub> _hub;
        public AuctionService(IUnitOfWork uow, ILogger<AuctionService> log, IHubContext<AuctionHub> hub)
        {
            _uow = uow;
            _logger = log;
            _hub = hub;
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

            var now = DateTime.UtcNow;
            var previewTime = dto.PreviewStartTime ?? dto.ScheduledStartTime;

            AuctionStatus status;
            if (dto.ScheduledStartTime > now)
            {
                status = previewTime <= now ? AuctionStatus.PreviewMode : AuctionStatus.Scheduled;
            }
            else
            {
                status = AuctionStatus.Active;
            }

            decimal reservePrice = dto.ReservePrice ?? dto.StartingPrice;
            bool isReserveMet = reservePrice <= dto.StartingPrice;
            DateTime? reserveMetAt = isReserveMet ? now : null;

            var auction = new Auction
            {
                VehicleId = dto.VehicleId,
                ScheduledStartTime = dto.ScheduledStartTime,
                StartUtc = dto.ScheduledStartTime,
                EndUtc = dto.EndUtc,
                StartingPrice = dto.StartingPrice,
                CurrentPrice = 0,
                ReservePrice = reservePrice,
                IsReserveMet = isReserveMet,
                ReserveMetAt = reserveMetAt,
                Status = status,
                PreviewStartTime = previewTime,
                CreatedUtc = now,
                UpdatedUtc = now
            };

            var createdAuction = await _uow.Auctions.AddAuctionAsync(auction);
            await _uow.SaveChangesAsync();

            var dtoResult = AuctionMapper.ToDTO(auction);
            return Result<AuctionDTO>.Success(dtoResult);
        }
        public async Task<Result<AuctionDTO?>> UpdateAuctionStatusAsync(int auctionId, AuctionStatus status)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            if (auction == null)
                return Result<AuctionDTO?>.Failure("Auction not found.");

            await _uow.Auctions.UpdateAuctionStatusAsync(auctionId, status);

            await _uow.SaveChangesAsync();
            return Result<AuctionDTO?>.Success(AuctionMapper.ToDTO(auction));
        }
        public async Task<List<AuctionDTO>> GetAuctionsAsync(AuctionQueryParams filters)
        {
            var query = _uow.Auctions.Query()
                .Include(a => a.Vehicle)
                .Include(a => a.Bids);

            var filteredQuery = AuctionQuery.ApplyFilters(query, filters);
            var sortedQuery = AuctionQuery.ApplySorting(filteredQuery, filters);

            var auctions = await sortedQuery
                .Where(a => a.Status != AuctionStatus.Scheduled)
                .AsNoTracking()
                .ToListAsync();

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

            if (auction.Status != AuctionStatus.Active || auction.EndUtc <= DateTime.UtcNow)
                return Result<BidDTO>.Failure("Auction has ended.");

            var bids = await _uow.Bids.GetBidsByAuctionIdAsync(auctionId);
            var errs = Validator.ValidateBidAmount(dto.Amount, auction.StartingPrice, auction.CurrentPrice, bids.Count);
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
            await _uow.Auctions.UpdateCurrentPriceAsync(auctionId, dto.Amount);
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

            if (auction.ReservePrice.HasValue && !auction.IsReserveMet && bid.Amount >= auction.ReservePrice.Value)
            {
                auction.IsReserveMet = true;
                auction.ReserveMetAt = DateTime.UtcNow;
                await _uow.Auctions.UpdateReserveStatusAsync(auctionId);
                await _uow.SaveChangesAsync();

                await _hub.Clients.Group($"auction-{auctionId}")
                    .SendAsync("ReservePriceMet");
            }

            var timeRemaining = auction.EndUtc - DateTime.UtcNow;
            if (timeRemaining.TotalMinutes <= auction.TriggerMinutes && auction.ExtensionCount < auction.MaxExtensions)
            {
                await _uow.Auctions.UpdateAuctionEndTimeAsync(auctionId, auction.ExtensionMinutes);
                await _uow.SaveChangesAsync();

                await _hub.Clients.Group($"auction-{auctionId}")
                    .SendAsync("AuctionExtended", auctionId);
            }

            await _hub.Clients.Group($"auction-{bid.AuctionId}")
                .SendAsync("ReceiveNewBid", bid.AuctionId);

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
        public async Task<Result<List<BidDTO>>> GetUserBidHistoryAsync(int userId)
        {
            if (await _uow.Users.GetUserByIdAsync(userId) is null)
                return Result<List<BidDTO>>.Failure("User not found.");

            var bids = await _uow.Bids.GetBidsByUserIdAsync(userId);

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
        public async Task<Result<List<WatchlistDTO>>> GetAuctionWatchersAsync(int auctionId)
        {
            if (await _uow.Auctions.GetAuctionByIdAsync(auctionId) is null)
                return Result<List<WatchlistDTO>>.Failure("Auction not found.");

            var watchlists = await _uow.Watchlist.GetAuctionWatchersAsync(auctionId);

            var dtos = watchlists.Select(w => new WatchlistDTO
            {
                WatchlistId = w.WatchlistId,
                UserId = w.UserId,
                AuctionId = w.AuctionId,
                CreatedUtc = w.CreatedUtc
            }).ToList();

            return Result<List<WatchlistDTO>>.Success(dtos);
        }
        public async Task<Result<int?>> GetHighestBidderIdAsync(int auctionId)
        {
            var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
            if (auction == null)
                return Result<int?>.Failure("Auction not found");

            var highestBidderId = await _uow.Bids.GetHighestBidderIdAsync(auctionId);
            return Result<int?>.Success(highestBidderId);
        }

    }
}