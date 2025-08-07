using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System.Threading.Tasks;

namespace AutoFiCore.Services;

public interface IVehicleService
{
    Task<VehicleListResult> GetAllVehiclesByStatusAsync(int pageView, int offset, string? status = null);
    Task<VehicleListResult> GetVehiclesByMakeAsync(int pageView, int offset, string make);
    Task<VehicleListResult> GetVehiclesByModelAsync(int pageView, int offset, string make);
    Task<List<string>> GetDistinctColorsAsync();
    Task<List<Vehicle>> SearchVehiclesAsync(VehicleFilterDto filters, int pageView, int offset, string? sortOrder = null);
    Task<int> GetTotalCountAsync(VehicleFilterDto filterDto);
    Task<Dictionary<string, int>> GetAvailableColorsCountAsync(VehicleFilterDto filterDto);
    Task<Dictionary<string, int>> GetGearboxCountsAsync(VehicleFilterDto filterDto);
    Task<List<VehicleModelJSON>> GetAllCarFeaturesAsync();
    VehicleModelJSON? GetCarFeature(List<VehicleModelJSON>? carFeatures, string make, string model);
    Task<List<string>> GetAllVehiclesMakesAsync();
    Task<List<string>> GetAllVehiclesCategoriesAsync();
    Task<Vehicle?> GetVehicleByIdAsync(int id);
    Task<Vehicle?> GetVehicleByVinAsync(string vin);
    Task<Vehicle?> CreateVehicleAsync(Vehicle vehicle);
    Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle);
    Task<bool> DeleteVehicleAsync(int id);
    Task<Questionnaire> SaveQuestionnaireAsync(QuestionnaireDTO dto);
    Task<List<VehicleOptionsDTO>> GetVehicleOptionsAsync();
}

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
    public VehicleModelJSON? GetCarFeature(List<VehicleModelJSON>? carFeatures, string make, string model)
    {
        if (carFeatures == null || carFeatures.Count == 0)
        return null;

        return _repository.GetCarFeature(carFeatures, make, model);
    }
    public async Task<List<VehicleModelJSON>> GetAllCarFeaturesAsync()
    {
        return await _repository.GetAllCarFeaturesAsync();
    }
    public async Task<List<string>> GetDistinctColorsAsync()
    {
        return await _cachedVehicleRepository.GetDistinctColorsAsync();
    }
    public async Task<List<VehicleOptionsDTO>> GetVehicleOptionsAsync()
    {
        return await _cachedVehicleRepository.GetVehicleOptionsAsync();
    }

    public async Task<VehicleListResult> GetAllVehiclesByStatusAsync(int pageView, int offset, string? status = null)
    {
        return await _repository.GetAllVehiclesByStatusAsync(pageView, offset, status);
    }
    public async Task<List<string>> GetAllVehiclesMakesAsync()
    {
        return await _repository.GetAllVehicleMakes();
    }
    public async Task<List<string>> GetAllVehiclesCategoriesAsync()
    {
        return await _repository.GetAllVehicleCategories();
    }
    public async Task<VehicleListResult> GetVehiclesByMakeAsync(int pageView, int offset, string make)
    {
        return await _repository.GetVehiclesByMakeAsync(pageView, offset, make);
    }
    public async Task<List<Vehicle>> SearchVehiclesAsync(VehicleFilterDto filters, int pageView, int offset, string? sortOrder = null)
    {
        return await _repository.SearchVehiclesAsync(filters, pageView, offset, sortOrder);
    }
    public async Task<Questionnaire> SaveQuestionnaireAsync(QuestionnaireDTO dto)
    {
        return await _repository.SaveQuestionnaireAsync(dto);
    }
    public async Task<int> GetTotalCountAsync(VehicleFilterDto filterDto)
    {
        return await _repository.GetTotalCountAsync(filterDto);
    }
    public async Task<Dictionary<string, int>> GetGearboxCountsAsync(VehicleFilterDto filterDto)
    { 
        return await _repository.GetGearboxCountsAsync(filterDto);
    }
    public async Task<Dictionary<string, int>> GetAvailableColorsCountAsync(VehicleFilterDto filterDto)
    {
        return await _repository.GetColorsCountsAsync(filterDto);
    }
    public async Task<VehicleListResult> GetVehiclesByModelAsync(int pageView, int offset, string model)
    {
        return await _repository.GetVehiclesByModelAsync(pageView, offset, model);
    }
    public async Task<Vehicle?> GetVehicleByIdAsync(int id)
    {
        return await _repository.GetVehicleByIdAsync(id);
    }
    public async Task<Vehicle?> GetVehicleByVinAsync(string vin)
    {
        return await _repository.GetVehicleByVinAsync(vin);
    }
    public async Task<Vehicle?> CreateVehicleAsync(Vehicle vehicle)
    {
        var result = await _unitOfWork.Vehicles.AddVehicleAsync(vehicle);
        await _unitOfWork.SaveChangesAsync();
        return result;
    }
    public async Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle)
    {
        var success = await _unitOfWork.Vehicles.UpdateVehicleAsync(vehicle);

        if (!success)
        {
            throw new InvalidOperationException($"Vehicle with ID {vehicle.Id} not found");
        }

        await _unitOfWork.SaveChangesAsync();
        return vehicle;
    }
    public async Task<bool> DeleteVehicleAsync(int id)
    {
        var result = await _unitOfWork.Vehicles.DeleteVehicleAsync(id);
        await _unitOfWork.SaveChangesAsync();
        return result;
    }
} 