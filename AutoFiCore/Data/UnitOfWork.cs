﻿using Microsoft.EntityFrameworkCore.Storage;

namespace AutoFiCore.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        public IVehicleRepository Vehicles { get; }
        public IUserRepository Users { get; }
        public IContactInfoRepository ContactInfo { get; }
        public IAuctionRepository Auctions { get; }
        public IBidRepository Bids { get; }
        public IWatchlistRepository Watchlist { get; }
        public INewsLetterRepository NewsLetter { get; }
        public IAutoBidRepository AutoBid { get; }
        public INotificationRepository Notification { get; }
        public IAnalyticsRepository Analytics { get; }
        public IReportRepository Report { get; }
        public IMetricsRepository Metrics { get; }
        public IPerformanceRepository Performance { get; }
        public UnitOfWork(ApplicationDbContext context, IVehicleRepository vehicleRepository, IUserRepository userRepository, IContactInfoRepository contactInfoRepository, INewsLetterRepository newsLetterRepository, IAuctionRepository auctions, IBidRepository bids, IWatchlistRepository watchlist, IAutoBidRepository autoBid, INotificationRepository notification, IAnalyticsRepository analytics, IReportRepository report, IMetricsRepository metrics, IPerformanceRepository performance)
        {
            _context = context;
            Vehicles = vehicleRepository;
            Users = userRepository;
            ContactInfo = contactInfoRepository;
            NewsLetter = newsLetterRepository;
            Auctions = auctions;
            Bids = bids;
            Watchlist = watchlist;
            AutoBid = autoBid;
            Notification = notification;
            Analytics = analytics;
            Report = report;
            Metrics = metrics;
            Performance = performance;
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
