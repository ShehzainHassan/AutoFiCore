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

    public DbSet<UserInteractions> UserInteractions { get; set; } = null!;

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
        modelBuilder.Entity<Vehicle>().HasIndex(v => new { v.Make, v.Model, v.Price, v.Year, v.Id}).HasDatabaseName("IX_Vehicles_Make_Model_Price_Year_Id");

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
    }
}