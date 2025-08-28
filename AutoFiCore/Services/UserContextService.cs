using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoFiCore.Services
{
    public interface IUserContextService
    {
        Task<UserContextDTO> GetUserContextAsync(int userId);
    }
    public class UserContextService : IUserContextService
    {
        private readonly IUnitOfWork _uow;
        public UserContextService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        private async Task<List<AuctionHistoryDTO>> GetUserAuctionHistoryAsync(int userId)
        {
            var userBids = await _uow.Bids.GetBidsByUserIdAsync(userId);

            var auctionIds = userBids.Select(b => b.AuctionId).Distinct().ToList();

            var auctionHistory = new List<AuctionHistoryDTO>();

            foreach (var auctionId in auctionIds)
            {
                var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
                if (auction == null) continue;

                var highestBidderId = await _uow.Bids.GetHighestBidderIdAsync(auctionId);

                bool isWinner = auction.Status == AuctionStatus.Ended &&
                                auction.CurrentPrice >= auction.ReservePrice &&
                                highestBidderId.HasValue &&
                                highestBidderId.Value == userId;

                auctionHistory.Add(new AuctionHistoryDTO
                {
                    AuctionId = auction.AuctionId,
                    VehicleId = auction.VehicleId,
                    StartUtc = auction.StartUtc,
                    EndUtc = auction.EndUtc,
                    CurrentPrice = auction.CurrentPrice,
                    Status = auction.Status.ToString(),
                    ReservePrice = auction.ReservePrice ?? auction.StartingPrice,
                    IsWinner = isWinner
                });
            }

            return auctionHistory;
        }
        public async Task<UserContextDTO> GetUserContextAsync(int userId)
        {
            var context = new UserContextDTO();
            context.SavedSearches = await _uow.Users.GetUserSavedSearches(userId);
            context.AuctionHistory = await GetUserAuctionHistoryAsync(userId);
            context.AutoBidSettings = await _uow.AutoBid.GetUserAutoBidSettingsAsync(userId);
            return context;
        }

    }
}
