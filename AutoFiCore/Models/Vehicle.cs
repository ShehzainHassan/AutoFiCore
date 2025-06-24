using System.ComponentModel.DataAnnotations;

namespace AutoFiCore.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required]
    [StringLength(17, MinimumLength = 17)]
    public string Vin { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [Range(1900, 2100)]
    public int Year { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Mileage { get; set; }

    [StringLength(30)]
    public string? Color { get; set; }

    [StringLength(20)]
    public string? FuelType { get; set; }

    [StringLength(20)]
    public string? Transmission { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }
    public Drivetrain? Drivetrain { get; set; } = null;
    public Engine? Engine { get; set; } = null;

    public FuelEconomy? FuelEconomy { get; set; } = null;

    public VehiclePerformance? VehiclePerformance { get; set; } = null;

    public Measurements? Measurements { get; set; } = null;

    public List<VehicleOptions>? VehicleOptions { get; set; } = null;
 }