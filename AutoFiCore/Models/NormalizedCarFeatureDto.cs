namespace AutoFiCore.DTOs;

public class NormalizedCarFeatureDto
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public NormalizedFeature Features { get; set; } = new();
}

public class NormalizedFeature
{
    public DrivetrainDto? Drivetrain { get; set; }
    public EngineDto? Engine { get; set; }
    public FuelEconomyDto? FuelEconomy { get; set; }
    public PerformanceDto? Performance { get; set; }
    public MeasurementsDto? Measurements { get; set; }
    public List<string>? Options { get; set; }
}

public class DrivetrainDto
{
    public string? Type { get; set; }
    public string? Transmission { get; set; }
}

public class EngineDto
{
    public string? Type { get; set; }
    public string? Size { get; set; }
    public decimal? Horsepower { get; set; }
    public decimal? TorqueFtLBS { get; set; }
    public decimal? TorqueRPM { get; set; }
    public decimal? Valves { get; set; }
    public string? CamType { get; set; }
}

public class FuelEconomyDto
{
    public decimal? FuelTankSize { get; set; }
    public decimal? CombinedMPG { get; set; }
    public decimal? CityMPG { get; set; }
    public decimal? HighwayMPG { get; set; }
    public decimal? CO2Emissions { get; set; }
}

public class PerformanceDto
{
    public decimal? ZeroTo60MPH { get; set; }
}

public class MeasurementsDto
{
    public int? Doors { get; set; }
    public decimal? MaximumSeating { get; set; }
    public decimal? HeightInches { get; set; }
    public decimal? WidthInches { get; set; }
    public decimal? LengthInches { get; set; }
    public decimal? WheelbaseInches { get; set; }
    public decimal? GroundClearance { get; set; }
    public decimal? CargoCapacityCuFt { get; set; }
    public decimal? CurbWeightLBS { get; set; }
}
