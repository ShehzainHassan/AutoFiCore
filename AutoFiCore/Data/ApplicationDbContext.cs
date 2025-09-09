using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<ContactInfo> ContactInfos { get; set; } = null!;
    public DbSet<UserSavedSearch> UserSavedSearches { get; set; } = null!;
    public DbSet<UserLikes> UserLikes { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Drivetrain> Drivetrains { get; set; } = null!;
    public DbSet<Engine> Engines { get; set; } = null!;
    public DbSet<FuelEconomy> FuelEconomies { get; set; } = null!;
    public DbSet<VehiclePerformance> VehiclePerformances { get; set; } = null!;
    public DbSet<Questionnaire> Questionnaires { get; set; } = null!;
    public DbSet<Measurements> Measurements { get; set; }
    public DbSet<VehicleOptions> VehicleOptions { get; set; }
    public DbSet<Newsletter> Newsletters { get; set; } = null!;
    public DbSet<UserInteractions> UserInteractions { get; set; } = null!;
    public DbSet<Auction> Auctions { get; set; } = null!;
    public DbSet<Bid> Bids { get; set; } = null!;
    public DbSet<Watchlist> Watchlists { get; set; } = null!;
    public DbSet<AutoBid> AutoBids { get; set; } = null!;
    public DbSet<BidStrategy> BidStrategies { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();
    public DbSet<DailyMetric> DailyMetrics => Set<DailyMetric>();
    public DbSet<AuctionAnalytics> AuctionAnalytics => Set<AuctionAnalytics>();
    public DbSet<PerformanceMetric> PerformanceMetrics => Set<PerformanceMetric>();
    public DbSet<APIPerformanceLog> ApiPerformanceLogs { get; set; }
    public DbSet<DBQueryLog> DbQueryLogs { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<RecentDownloads> RecentDownloads { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<PopularQueries> PopularQueries { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        //Configure constraints
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Vin).IsRequired().HasMaxLength(17);
            entity.Property(e => e.Make).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Year).IsRequired();
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.Mileage).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(30);
            entity.Property(e => e.FuelType).HasMaxLength(20);
            entity.Property(e => e.Transmission).HasMaxLength(20);
        });

        modelBuilder.Entity<ContactInfo>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.FirstName).IsRequired().HasMaxLength(20);
            entity.Property(c => c.LastName).IsRequired().HasMaxLength(20);
            entity.Property(c => c.SelectedOption).IsRequired().HasMaxLength(30);
            entity.Property(c => c.VehicleName).IsRequired().HasMaxLength(100);
            entity.Property(c => c.PostCode).IsRequired();
            entity.Property(c => c.Email).IsRequired();
            entity.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(c => c.PreferredContactMethod).IsRequired().HasMaxLength(20);
            entity.Property(c => c.Comment).HasMaxLength(100);
            entity.Property(c => c.EmailMeNewResults).IsRequired();
        });
        modelBuilder.Entity<Questionnaire>(entity =>
        {
            entity.HasKey(q => q.Id);
        });

        modelBuilder.Entity<UserInteractions>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.InteractionType).IsRequired().HasMaxLength(50);
            entity.Property(u => u.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(u => u.User).WithMany().HasForeignKey(u => u.UserId);
            entity.HasOne(u => u.Vehicle).WithMany().HasForeignKey(u => u.VehicleId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(40);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(25);
            entity.Property(u => u.Password).IsRequired().HasMaxLength(100);
            entity.Property(u => u.CreatedUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(u => u.LastLoggedIn).IsRequired();
        });

        modelBuilder.Entity<UserLikes>(entity =>
        {
            entity.HasKey(ul => new { ul.userId, ul.vehicleVin });
            entity.Property(ul => ul.vehicleVin).IsRequired().HasMaxLength(17);
        });

        modelBuilder.Entity<UserSavedSearch>(entity =>
        {
            entity.HasKey(us => new { us.userId, us.search });
            entity.Property(us => us.search).IsRequired();
        });

        modelBuilder.Entity<Drivetrain>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Transmission).IsRequired().HasMaxLength(50);
            ;
        });

        modelBuilder.Entity<Engine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(25);
            entity.Property(e => e.Size).HasMaxLength(4);
            entity.Property(e => e.Horsepower).IsRequired();
            entity.Property(e => e.TorqueFtLBS);
            entity.Property(e => e.TorqueRPM);
            entity.Property(e => e.Valves);
            entity.Property(e => e.CamType).HasMaxLength(25);
        });

        modelBuilder.Entity<FuelEconomy>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.FuelTankSize);
            entity.Property(f => f.CombinedMPG).IsRequired();
            entity.Property(f => f.CityMPG).IsRequired();
            entity.Property(f => f.HighwayMPG).IsRequired();
            entity.Property(f => f.CO2Emissions).IsRequired();
        });

        modelBuilder.Entity<VehiclePerformance>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.ZeroTo60MPH).IsRequired();
        });

        modelBuilder.Entity<Measurements>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Doors).IsRequired();
            entity.Property(v => v.MaximumSeating).IsRequired();
            entity.Property(v => v.HeightInches).IsRequired();
            entity.Property(v => v.WidthInches).IsRequired();
            entity.Property(v => v.LengthInches).IsRequired();
            entity.Property(v => v.WheelbaseInches).IsRequired();
            entity.Property(v => v.GroundClearance).IsRequired();
            entity.Property(v => v.CargoCapacityCuFt);
            entity.Property(v => v.CurbWeightLBS).IsRequired();
        });

        modelBuilder.Entity<VehicleOptions>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Options).IsRequired();
        });

        modelBuilder.Entity<Newsletter>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Email).IsRequired().HasMaxLength(25);
        });

        modelBuilder.Entity<Auction>(entity =>
        {
            entity.HasKey(a => a.AuctionId);
            entity.Property(a => a.VehicleId).IsRequired();
            entity.Property(a => a.StartUtc).IsRequired();
            entity.Property(a => a.EndUtc).IsRequired();
            entity.Property(a => a.StartingPrice).IsRequired();
            entity.Property(a => a.CurrentPrice).IsRequired();
            entity.Property(a => a.Status).IsRequired().HasMaxLength(25).HasConversion<string>();
            entity.Property(a => a.CreatedUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(a => a.UpdatedUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasKey(b => b.BidId);
            entity.Property(b => b.Amount).IsRequired();
            entity.Property(b => b.IsAuto).HasDefaultValue(false);
            entity.Property(b => b.CreatedUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Watchlist>(entity =>
        {
            entity.HasKey(w => w.WatchlistId);
            entity.Property(w => w.CreatedUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<AutoBid>(entity =>
        {
            entity.HasKey(ab => ab.Id);
            entity.Property(ab => ab.MaxBidAmount).IsRequired();
            entity.Property(ab => ab.CurrentBidAmount).IsRequired();
            entity.Property(ab => ab.IsActive).IsRequired();
            entity.Property(ab => ab.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(ab => ab.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<BidStrategy>(entity =>
        {
            entity.HasKey(b => new { b.AuctionId, b.UserId });
            entity.Property(b => b.Type).IsRequired();
            entity.Property(b => b.PreferredBidTiming).IsRequired();
            entity.Property(b => b.SuccessfulBids).IsRequired().HasDefaultValue(0);
            entity.Property(b => b.FailedBids).IsRequired().HasDefaultValue(0);
            entity.Property(b => b.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(b => b.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.NotificationType).HasConversion<string>().IsRequired();
            entity.Property(n => n.Priority).HasConversion<string>().IsRequired();
            entity.Property(n => n.Title).IsRequired().HasMaxLength(255);
            entity.Property(n => n.Message).IsRequired(false);
            entity.Property(n => n.IsRead).HasDefaultValue(false);
            entity.Property(n => n.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(n => n.EmailSentAt).IsRequired(false);
        });

        modelBuilder.Entity<AnalyticsEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasConversion<string>().IsRequired();
            entity.Property(e => e.Source).HasConversion<string>();
            entity.Property(e => e.EventData).HasColumnType("text");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<DailyMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.MetricType).HasConversion<string>().IsRequired();
            entity.Property(e => e.Value).HasColumnType("decimal(15,2)");
            entity.Property(e => e.Count);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<AuctionAnalytics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalViews).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.UniqueBidders).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.TotalBids).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.ViewToBidRatio).HasColumnType("decimal(5,2)");
            entity.Property(e => e.StartPrice).HasColumnType("decimal(15,2)");
            entity.Property(e => e.FinalPrice).HasColumnType("decimal(15,2)");
            entity.Property(e => e.Duration);
            entity.Property(e => e.CompletionStatus);
            entity.Property(e => e.SuccessRate);
            entity.Property(e => e.EngagementScore);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<PerformanceMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetricType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Endpoint).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ResponseTime).IsRequired();
            entity.Property(e => e.StatusCode).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<APIPerformanceLog>(entity =>
        {
            entity.Property(e => e.Endpoint).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ResponseTime).IsRequired();
            entity.Property(e => e.StatusCode).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();

        });

        modelBuilder.Entity<DBQueryLog>(entity =>
        {
            entity.Property(e => e.QueryType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Duration).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.Property(e => e.ErrorCode).IsRequired();
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP"); ;
        });

        modelBuilder.Entity<RecentDownloads>(entity =>
        {
            entity.HasKey(rd => rd.Id);
            entity.Property(rd => rd.ReportType).IsRequired().HasConversion<string>();
            entity.Property(rd => rd.DateRange).IsRequired();
            entity.Property(rd => rd.Format).HasDefaultValue("CSV");
            entity.Property(rd => rd.DownloadedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChatSessionId).IsRequired();
            entity.Property(e => e.Sender).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UiType).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.QueryType).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.SuggestedActions).HasColumnType("jsonb").IsRequired(false);
            entity.Property(e => e.Sources).HasColumnType("jsonb").IsRequired(false);
            entity.Property(e => e.Feedback).HasConversion<string>().HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<PopularQueries>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.DisplayText).IsRequired().HasMaxLength(250);
            entity.Property(p => p.Count).HasDefaultValue(1);
            entity.Property(p => p.LastAsked).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(q => q.Embedding).HasColumnType("double precision[]");

        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.UserId).IsRequired();
            entity.Property(r => r.Token).IsRequired().HasMaxLength(256);
            entity.Property(r => r.Expires).IsRequired();
            entity.Property(r => r.IsRevoked).IsRequired();
            entity.Property(r => r.Created).IsRequired();
        });

        // Configure indexes
        modelBuilder.Entity<Vehicle>().HasIndex(v => v.Make).HasDatabaseName("IX_Vehicles_Make");
        modelBuilder.Entity<Vehicle>().HasIndex(v => v.Model).HasDatabaseName("IX_Vehicles_Model");
        modelBuilder.Entity<Vehicle>().HasIndex(v => v.Price).HasDatabaseName("IX_Vehicles_Price");
        modelBuilder.Entity<Vehicle>().HasIndex(v => v.Year).HasDatabaseName("IX_Vehicles_Year");
        modelBuilder.Entity<Vehicle>().HasIndex(v => v.Mileage).HasDatabaseName("IX_Vehicles_Mileage");
        modelBuilder.Entity<Vehicle>().HasIndex(v => v.Transmission).HasDatabaseName("IX_Vehicles_Transmission");
        modelBuilder.Entity<Vehicle>().HasIndex(v => v.Color).HasDatabaseName("IX_Vehicles_Color");
        modelBuilder.Entity<Vehicle>().HasIndex(v => v.Status).HasDatabaseName("IX_Vehicles_Status");

        modelBuilder.Entity<Vehicle>().HasIndex(v => new { v.Make, v.Id }).HasDatabaseName("IX_Vehicles_Make_Id");
        modelBuilder.Entity<Vehicle>().HasIndex(v => new { v.Model, v.Id }).HasDatabaseName("IX_Vehicles_Model_Id");
        modelBuilder.Entity<Vehicle>().HasIndex(v => new { v.Price, v.Id }).HasDatabaseName("IX_Vehicles_Price_Id");
        modelBuilder.Entity<Vehicle>().HasIndex(v => new { v.Year, v.Id }).HasDatabaseName("IX_Vehicles_Year_Id");
        modelBuilder.Entity<Vehicle>().HasIndex(v => new { v.Make, v.Model, v.Price, v.Year, v.Id }).HasDatabaseName("IX_Vehicles_Make_Model_Price_Year_Id");

        modelBuilder.Entity<Auction>().HasIndex(a => a.VehicleId).IsUnique().HasDatabaseName("IX_Auction_Vehicle_Id_Unique");
        modelBuilder.Entity<Auction>().HasIndex(a => a.Status);

        modelBuilder.Entity<Bid>().HasIndex(b => b.AuctionId).HasDatabaseName("IX_Bid_AuctionId");
        modelBuilder.Entity<Bid>().HasIndex(b => b.UserId).HasDatabaseName("IX_Bid_UserId");
        modelBuilder.Entity<Bid>().HasIndex(b => new { b.AuctionId, b.CreatedUtc }).HasDatabaseName("IX_Bid_AuctionId_CreatedUtc");

        modelBuilder.Entity<Watchlist>().HasIndex(w => new { w.UserId, w.AuctionId }).IsUnique().HasDatabaseName("IX_Watchlist_UserId_AuctionId");

        modelBuilder.Entity<AutoBid>().HasIndex(ab => new { ab.AuctionId, ab.IsActive, ab.MaxBidAmount }).HasDatabaseName("IX_AutoBid_ActiveByAuction");

        modelBuilder.Entity<BidStrategy>().HasIndex(b => b.Type).HasDatabaseName("IX_BidStrategy_Type");
        modelBuilder.Entity<BidStrategy>().HasIndex(b => b.PreferredBidTiming).HasDatabaseName("IX_BidStrategy_Timing");

        modelBuilder.Entity<AnalyticsEvent>().HasIndex(e => e.EventType);
        modelBuilder.Entity<AnalyticsEvent>().HasIndex(e => e.UserId);
        modelBuilder.Entity<AnalyticsEvent>().HasIndex(e => e.AuctionId);
        modelBuilder.Entity<AnalyticsEvent>().HasIndex(e => e.CreatedAt);

        modelBuilder.Entity<DailyMetric>().HasIndex(e => new { e.Date, e.MetricType });
        modelBuilder.Entity<DailyMetric>().HasIndex(e => e.Category);

        modelBuilder.Entity<AuctionAnalytics>().HasIndex(e => e.AuctionId).IsUnique();
        modelBuilder.Entity<AuctionAnalytics>().HasIndex(e => e.UpdatedAt);

        modelBuilder.Entity<PerformanceMetric>().HasIndex(e => e.MetricType);
        modelBuilder.Entity<PerformanceMetric>().HasIndex(e => e.CreatedAt);
        modelBuilder.Entity<PerformanceMetric>().HasIndex(e => e.StatusCode);

        modelBuilder.Entity<APIPerformanceLog>().HasIndex(e => e.Timestamp);
        modelBuilder.Entity<APIPerformanceLog>().HasIndex(e => e.Endpoint);

        modelBuilder.Entity<DBQueryLog>().HasIndex(e => e.Timestamp);
        modelBuilder.Entity<DBQueryLog>().HasIndex(e => e.QueryType);

        modelBuilder.Entity<ErrorLog>().HasIndex(e => e.Timestamp);
        modelBuilder.Entity<ErrorLog>().HasIndex(e => e.ErrorCode);

        modelBuilder.Entity<ChatSession>().HasIndex(e => e.UserId).HasDatabaseName("IX_ChatSessions_UserId");
        modelBuilder.Entity<ChatSession>().HasIndex(e => e.CreatedAt).HasDatabaseName("IX_ChatSessions_CreatedAt");

        modelBuilder.Entity<ChatMessage>().HasIndex(e => e.ChatSessionId).HasDatabaseName("IX_ChatMessages_ChatSessionId");
        modelBuilder.Entity<ChatMessage>().HasIndex(e => e.Timestamp).HasDatabaseName("IX_ChatMessages_Timestamp");
        modelBuilder.Entity<ChatMessage>().HasIndex(e => e.Sender).HasDatabaseName("IX_ChatMessages_Sender");

        modelBuilder.Entity<PopularQueries>().HasIndex(e => new { e.Count, e.LastAsked });

        modelBuilder.Entity<RefreshToken>().HasIndex(r => r.Token).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(r => r.UserId);

        // Configure relationships and set up cascade delete
        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Drivetrain)
            .WithOne(d => d.Vehicle)
            .HasForeignKey<Drivetrain>(d => d.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Engine)
            .WithOne(e => e.Vehicle)
            .HasForeignKey<Engine>(e => e.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.FuelEconomy)
            .WithOne(f => f.Vehicle)
            .HasForeignKey<FuelEconomy>(f => f.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.VehiclePerformance)
            .WithOne(p => p.Vehicle)
            .HasForeignKey<VehiclePerformance>(p => p.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Measurements)
            .WithOne(m => m.Vehicle)
            .HasForeignKey<Measurements>(m => m.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vehicle>()
            .HasMany(v => v.VehicleOptions)
            .WithMany(o => o.Vehicle)
            .UsingEntity(j => j.ToTable("Vehicle_VehicleOptions_Mapping"));

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Auction)
            .WithOne(a => a.Vehicle)
            .HasForeignKey<Auction>(a => a.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bid>()
            .HasOne(b => b.Auction)
            .WithMany(a => a.Bids)
            .HasForeignKey(b => b.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Bid>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bids)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Watchlist>().HasOne(w => w.User)
            .WithMany(u => u.Watchlists)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Watchlist>().HasOne(w => w.Auction)
            .WithMany(a => a.Watchers)
            .HasForeignKey(w => w.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AutoBid>()
            .HasOne(ab => ab.Auction)
            .WithMany(a => a.AutoBids)
            .HasForeignKey(ab => ab.AuctionId);

        modelBuilder.Entity<AutoBid>()
            .HasOne(ab => ab.User)
            .WithMany(u => u.AutoBids)
            .HasForeignKey(ab => ab.UserId);

        modelBuilder.Entity<BidStrategy>()
            .HasOne(b => b.Auction)
            .WithMany(a => a.BidStrategies)
            .HasForeignKey(b => b.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BidStrategy>()
            .HasOne(bs => bs.User)
            .WithMany(u => u.BidStrategies)
            .HasForeignKey(bs => bs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>().HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>().HasOne(n => n.Auction)
            .WithMany(a => a.Notifications)
            .HasForeignKey(n => n.AuctionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AnalyticsEvent>().HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AnalyticsEvent>().HasOne(e => e.Auction)
            .WithMany()
            .HasForeignKey(e => e.AuctionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AuctionAnalytics>()
            .HasOne(e => e.Auction)
            .WithOne(a => a.AuctionAnalytics)
            .HasForeignKey<AuctionAnalytics>(e => e.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ChatMessage>().HasOne(e => e.ChatSession)
            .WithMany(s => s.Messages)
            .HasForeignKey(e => e.ChatSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}