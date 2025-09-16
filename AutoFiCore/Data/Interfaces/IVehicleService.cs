using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IVehicleService
    {
        Task<Result<VehicleListResult>> GetAllVehiclesByStatusAsync(int pageView, int offset, string? status = null);
        Task<Result<VehicleListResult>> GetVehiclesByMakeAsync(int pageView, int offset, string make);
        Task<Result<VehicleListResult>> GetVehiclesByModelAsync(int pageView, int offset, string model);
        Task<Result<List<string>>> GetDistinctColorsAsync();
        Task<Result<List<Vehicle>>> SearchVehiclesAsync(VehicleFilterDto filters, int pageView, int offset, string? sortOrder = null);
        Task<Result<int>> GetTotalCountAsync(VehicleFilterDto filterDto);
        Task<Result<Dictionary<string, int>>> GetAvailableColorsCountAsync(VehicleFilterDto filterDto);
        Task<Result<Dictionary<string, int>>> GetGearboxCountsAsync(VehicleFilterDto filterDto);
        Task<Result<List<VehicleModelJSON>>> GetAllCarFeaturesAsync();
        Result<VehicleModelJSON> GetCarFeature(List<VehicleModelJSON>? carFeatures, string make, string model);
        Task<Result<List<string>>> GetAllVehiclesMakesAsync();
        Task<Result<List<string>>> GetAllVehiclesCategoriesAsync();
        Task<Result<Vehicle>> GetVehicleByIdAsync(int id);
        Task<Result<Vehicle>> GetVehicleByVinAsync(string vin);
        Task<Result<Vehicle>> CreateVehicleAsync(Vehicle vehicle);
        Task<Result<Vehicle>> UpdateVehicleAsync(Vehicle vehicle);
        Task<Result<bool>> DeleteVehicleAsync(int id);
        Task<Result<Questionnaire>> SaveQuestionnaireAsync(QuestionnaireDTO dto);
        Task<Result<List<VehicleOptionsDTO>>> GetVehicleOptionsAsync();
    }
}
