using AutoFiCore.Dto;
using AutoFiCore.Models;

namespace AutoFiCore.Data;

public interface IVehicleRepository
{
    Task<VehicleListResult> GetAllVehiclesByStatusAsync(int pageView, int offset, string? status = null);
    Task<VehicleListResult> GetVehiclesByMakeAsync(int pageView, int offset, string make);
    Task<VehicleListResult> GetVehiclesByModelAsync(int pageView, int offset, string model);
    Task<List<string>> GetDistinctColorsAsync();
    Task<List<Vehicle>> SearchVehiclesAsync(VehicleFilterDto filters, int pageView, int offset, string? sortOrder = null);
    VehicleModelJSON? GetCarFeature(List<VehicleModelJSON> carFeatures, string make, string model);
    Task<List<VehicleModelJSON>> GetAllCarFeaturesAsync();
    Task<List<string>> GetAllVehicleMakes();
    Task<Vehicle?> GetVehicleByIdAsync(int id);
    Task<Vehicle?> GetVehicleByVinAsync(string Vin);
    Task<Vehicle> AddVehicleAsync(Vehicle vehicle);
    Task<bool> UpdateVehicleAsync(Vehicle vehicle);
    Task<bool> DeleteVehicleAsync(int id);
    Task<int> GetTotalCountAsync(VehicleFilterDto filterDto);
    Task<Dictionary<string, int>> GetGearboxCountsAsync(VehicleFilterDto filterDto);
    Task<Dictionary<string, int>> GetColorsCountsAsync(VehicleFilterDto filterDto);
    Task<Questionnaire> SaveQuestionnaireAsync(QuestionnaireDTO dto);
    Task<List<VehicleOptionsDTO>> GetVehicleOptionsAsync();
    public bool VehicleExists(int id);

}