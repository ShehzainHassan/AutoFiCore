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
            entity.Property(a => a.Status).IsRequired().HasMaxLength(25);
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
    }
}