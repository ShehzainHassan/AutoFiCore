﻿using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace AutoFiCore.Data
{
    public interface IAutoBidRepository
    {
        Task<AutoBid> AddAutoBidAsync(AutoBid autoBid);
        Task<bool> IsActiveAsync(int userId, int auctionId);
        Task<AutoBid?> GetByIdAsync(int userId, int auctionId);
        Task SetInactiveAsync(int userId, int auctionId);
        Task<BidStrategy?> GetBidStrategyByUserAndAuctionAsync(int userId, int auctionId);
        Task<CreateAutoBidDTO?> GetAutoBidWithStrategyAsync(int userId, int auctionId);
        Task<List<CreateAutoBidDTO>> GetActiveAutoBidsWithStrategyByUserAsync(int userId);
        Task<List<AutoBid>> GetActiveAutoBidsByAuctionIdAsync(int auctionId);
        Task<List<AutoBid>> GetEligibleAutoBidsAsync(int auctionId, decimal currentBid);
        Task<BidStrategy> AddBidStrategyAsync(BidStrategy bidStrategy);
        public void UpdateBidStrategy(BidStrategy updatedStrategy);
    } 
}
