using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Polly;
using System.Threading.Tasks;

namespace AutoFiCore.Services;
public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _repository;
    private readonly CachedVehicleRepository _cachedVehicleRepository;
    private readonly ILogger<VehicleService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    public VehicleService(IVehicleRepository repository, ILogger<VehicleService> logger, CachedVehicleRepository cachedVehicleRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _cachedVehicleRepository = cachedVehicleRepository;
        _unitOfWork = unitOfWork;
    }
    public Result<VehicleModelJSON> GetCarFeature(List<VehicleModelJSON>? carFeatures, string make, string model)
    {
        try
        {
            if (carFeatures == null || carFeatures.Count == 0)
                return Result<VehicleModelJSON>.Failure("Car features list is empty or null.");

            var feature = _repository.GetCarFeature(carFeatures, make, model);

            return feature != null
                ? Result<VehicleModelJSON>.Success(feature)
                : Result<VehicleModelJSON>.Failure($"Feature not found for make '{make}' and model '{model}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve car feature. Make={Make}, Model={Model}", make, model);
            return Result<VehicleModelJSON>.Failure("Error retrieving car feature.");
        }
    }
    public async Task<Result<List<VehicleModelJSON>>> GetAllCarFeaturesAsync()
    {
        try
        {
            var features = await _repository.GetAllCarFeaturesAsync();
            return Result<List<VehicleModelJSON>>.Success(features);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all car features.");
            return Result<List<VehicleModelJSON>>.Failure("Error retrieving car features.");
        }
    }
    public async Task<Result<List<string>>> GetDistinctColorsAsync()
    {
        try
        {
            var colors = await _cachedVehicleRepository.GetDistinctColorsAsync();
            return Result<List<string>>.Success(colors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve distinct vehicle colors.");
            return Result<List<string>>.Failure("Error retrieving vehicle colors.");
        }
    }
    public async Task<Result<List<VehicleOptionsDTO>>> GetVehicleOptionsAsync()
    {
        try
        {
            var options = await _cachedVehicleRepository.GetVehicleOptionsAsync();
            return Result<List<VehicleOptionsDTO>>.Success(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve vehicle options.");
            return Result<List<VehicleOptionsDTO>>.Failure("Error retrieving vehicle options.");
        }
    }
    public async Task<Result<VehicleListResult>> GetAllVehiclesByStatusAsync(int pageView, int offset, string? status = null)
    {
        try
        {
            var result = await _repository.GetAllVehiclesByStatusAsync(pageView, offset, status);
            return Result<VehicleListResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve vehicles by status. Status={Status}", status);
            return Result<VehicleListResult>.Failure("Error retrieving vehicles by status.");
        }
    }
    public async Task<Result<List<string>>> GetAllVehiclesMakesAsync()
    {
        try
        {
            var makes = await _repository.GetAllVehicleMakes();
            return Result<List<string>>.Success(makes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve vehicle makes.");
            return Result<List<string>>.Failure("Error retrieving vehicle makes.");
        }
    }
    public async Task<Result<List<string>>> GetAllVehiclesCategoriesAsync()
    {
        try
        {
            var categories = await _repository.GetAllVehicleCategories();
            return Result<List<string>>.Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve vehicle categories.");
            return Result<List<string>>.Failure("Error retrieving vehicle categories.");
        }
    }
    public async Task<Result<VehicleListResult>> GetVehiclesByMakeAsync(int pageView, int offset, string make)
    {
        try
        {
            var result = await _repository.GetVehiclesByMakeAsync(pageView, offset, make);
            return Result<VehicleListResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve vehicles by make. Make={Make}", make);
            return Result<VehicleListResult>.Failure("Error retrieving vehicles by make.");
        }
    }
    public async Task<Result<List<Vehicle>>> SearchVehiclesAsync(VehicleFilterDto filters, int pageView, int offset, string? sortOrder = null)
    {
        try
        {
            var vehicles = await _repository.SearchVehiclesAsync(filters, pageView, offset, sortOrder);
            return Result<List<Vehicle>>.Success(vehicles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search vehicles. Filters={@Filters}, SortOrder={SortOrder}", filters, sortOrder);
            return Result<List<Vehicle>>.Failure("Error searching vehicles.");
        }
    }
    public async Task<Result<Questionnaire>> SaveQuestionnaireAsync(QuestionnaireDTO dto)
    {
        var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _repository.SaveQuestionnaireAsync(dto);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Result<Questionnaire>.Success(result);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<Questionnaire>.Failure("Failed to save questionnaire.");
            }
        });
    }
    public async Task<Result<int>> GetTotalCountAsync(VehicleFilterDto filterDto)
    {
        try
        {
            var count = await _repository.GetTotalCountAsync(filterDto);
            return Result<int>.Success(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve total vehicle count.");
            return Result<int>.Failure("Error retrieving vehicle count.");
        }
    }
    public async Task<Result<Dictionary<string, int>>> GetGearboxCountsAsync(VehicleFilterDto filterDto)
    {
        try
        {
            var counts = await _repository.GetGearboxCountsAsync(filterDto);
            return Result<Dictionary<string, int>>.Success(counts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve gearbox counts.");
            return Result<Dictionary<string, int>>.Failure("Error retrieving gearbox counts.");
        }
    }
    public async Task<Result<Dictionary<string, int>>> GetAvailableColorsCountAsync(VehicleFilterDto filterDto)
    {
        try
        {
            var counts = await _repository.GetColorsCountsAsync(filterDto);
            return Result<Dictionary<string, int>>.Success(counts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve available color counts.");
            return Result<Dictionary<string, int>>.Failure("Error retrieving color counts.");
        }
    }
    public async Task<Result<VehicleListResult>> GetVehiclesByModelAsync(int pageView, int offset, string model)
    {
        try
        {
            var result = await _repository.GetVehiclesByModelAsync(pageView, offset, model);
            return Result<VehicleListResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve vehicles by model. Model={Model}", model);
            return Result<VehicleListResult>.Failure("Error retrieving vehicles by model.");
        }
    }
    public async Task<Result<Vehicle>> GetVehicleByIdAsync(int id)
    {
        try
        {
            var vehicle = await _repository.GetVehicleByIdAsync(id);
            return vehicle != null
                ? Result<Vehicle>.Success(vehicle)
                : Result<Vehicle>.Failure($"Vehicle with ID {id} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve vehicle by ID. ID={Id}", id);
            return Result<Vehicle>.Failure("Error retrieving vehicle by ID.");
        }
    }
    public async Task<Result<Vehicle>> GetVehicleByVinAsync(string vin)
    {
        try
        {
            var vehicle = await _repository.GetVehicleByVinAsync(vin);
            return vehicle != null
                ? Result<Vehicle>.Success(vehicle)
                : Result<Vehicle>.Failure($"Vehicle with VIN {vin} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve vehicle by VIN. VIN={Vin}", vin);
            return Result<Vehicle>.Failure("Error retrieving vehicle by VIN.");
        }
    }
    public async Task<Result<Vehicle>> CreateVehicleAsync(Vehicle vehicle)
    {
        var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _unitOfWork.Vehicles.AddVehicleAsync(vehicle);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Result<Vehicle>.Success(result);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Failed to create vehicle. VIN={Vin}", vehicle.Vin);
                return Result<Vehicle>.Failure("Failed to create vehicle.");
            }
        });
    }
    public async Task<Result<Vehicle>> UpdateVehicleAsync(Vehicle vehicle)
    {
        var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var success = await _unitOfWork.Vehicles.UpdateVehicleAsync(vehicle);
                if (!success)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<Vehicle>.Failure($"Vehicle with ID {vehicle.Id} not found.");
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Result<Vehicle>.Success(vehicle);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Failed to update vehicle. ID={Id}", vehicle.Id);
                return Result<Vehicle>.Failure("Failed to update vehicle.");
            }
        });
    }
    public async Task<Result<bool>> DeleteVehicleAsync(int id)
    {
        var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _unitOfWork.Vehicles.DeleteVehicleAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(result);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Failed to delete vehicle. ID={Id}", id);
                return Result<bool>.Failure("Failed to delete vehicle.");
            }
        });
    }
} 