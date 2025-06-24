using AutoFiCore.Dto;
using AutoFiCore.DTOs;
using AutoFiCore.Models;

namespace AutoFiCore.Utilities
{
    public class NormalizeInput
    {
        public static string? NormalizeMakeModel(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            input = input.Trim();

            if (input.Equals("Any_Makes", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("Any_Models", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return input;
        }
        public static string? NormalizeStatus(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;
            if (input.Equals("Any", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            input = input.Trim().ToUpper();
            return input;
        }

        public static string? NormalizeGearboxColors(string? input)
        {
            if (string.IsNullOrWhiteSpace(input) ||
                string.Equals(input.Trim(), "Any", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return input.Trim();
        }
        public static NormalizedCarFeatureDto NormalizeCarFeatures(VehicleModelJSON vehicle)
        {
            return new NormalizedCarFeatureDto
            {
                Make = vehicle.Make!,
                Model = vehicle.Model!,
                Year = vehicle.Year,
                Features = new NormalizedFeature
                {
                    Drivetrain = vehicle.Features?.Drivetrain == null ? null : new DrivetrainDto
                    {
                        Type = vehicle.Features.Drivetrain.Type,
                        Transmission = vehicle.Features.Drivetrain.Transmission
                    },
                    Engine = vehicle.Features?.Engine == null ? null : new EngineDto
                    {
                        Type = vehicle.Features.Engine.Type,
                        Size = vehicle.Features.Engine.Size,
                        Horsepower = vehicle.Features.Engine.Horsepower,
                        TorqueFtLBS = vehicle.Features.Engine.TorqueFtLBS,
                        TorqueRPM = vehicle.Features.Engine.TorqueRPM,
                        Valves = vehicle.Features.Engine.Valves,
                        CamType = vehicle.Features.Engine.CamType
                    },
                    FuelEconomy = vehicle.Features?.FuelEconomy == null ? null : new FuelEconomyDto
                    {
                        FuelTankSize = vehicle.Features.FuelEconomy.FuelTankSize,
                        CombinedMPG = vehicle.Features.FuelEconomy.CombinedMPG,
                        CityMPG = vehicle.Features.FuelEconomy.CityMPG,
                        HighwayMPG = vehicle.Features.FuelEconomy.HighwayMPG,
                        CO2Emissions = vehicle.Features.FuelEconomy.CO2Emissions
                    },
                    Performance = vehicle.Features?.Performance == null ? null : new PerformanceDto
                    {
                        ZeroTo60MPH = vehicle.Features.Performance.ZeroTo60MPH
                    },
                    Measurements = vehicle.Features?.Measurements == null ? null : new MeasurementsDto
                    {
                        Doors = vehicle.Features.Measurements.Doors,
                        MaximumSeating = vehicle.Features.Measurements.MaximumSeating,
                        HeightInches = vehicle.Features.Measurements.HeightInches,
                        WidthInches = vehicle.Features.Measurements.WidthInches,
                        LengthInches = vehicle.Features.Measurements.LengthInches,
                        WheelbaseInches = vehicle.Features.Measurements.WheelbaseInches,
                        GroundClearance = vehicle.Features.Measurements.GroundClearance,
                        CargoCapacityCuFt = vehicle.Features.Measurements.CargoCapacityCuFt,
                        CurbWeightLBS = vehicle.Features.Measurements.CurbWeightLBS
                    },
                    Options = vehicle.Features?.Options
                }
            };
        }
        public static VehicleFilterDto NormalizeFilters(VehicleFilterDto filters)
        {
            filters.Make = NormalizeMakeModel(filters.Make);
            filters.Model = NormalizeMakeModel(filters.Model);
            filters.Gearbox = NormalizeGearboxColors(filters.Gearbox);
            filters.SelectedColors = NormalizeGearboxColors(filters.SelectedColors);
            filters.Status = NormalizeStatus(filters.Status);

            return filters;
        }
    }
}
