using Microsoft.EntityFrameworkCore;
using AutoFiCore.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using AutoFiCore.Utilities;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using AutoFiCore.Dto;

namespace AutoFiCore.Data;

public class DbVehicleRepository : IVehicleRepository
{

    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DbVehicleRepository> _logger;
    private readonly IWebHostEnvironment _hostingEnvironment;
    public DbVehicleRepository(ApplicationDbContext dbContext, ILogger<DbVehicleRepository> logger, IWebHostEnvironment hostingEnvironment)
    {
        _dbContext = dbContext;
        _logger = logger;
        _hostingEnvironment = hostingEnvironment;
    }
    public VehicleModelJSON? GetCarFeature(List<VehicleModelJSON>? carFeatures, string make, string model)
    {
        if (carFeatures == null)
            return null;

        return carFeatures.FirstOrDefault(c => string.Equals(c.Make, make, StringComparison.OrdinalIgnoreCase) && string.Equals(c.Model, model, StringComparison.OrdinalIgnoreCase));
    }
    public async Task<List<VehicleModelJSON>> GetAllCarFeaturesAsync()
    {
        var rootPath = _hostingEnvironment.ContentRootPath;
        var fullPath = Path.Combine(rootPath, "Data", "car-features.json");

        if (!System.IO.File.Exists(fullPath))
        {
            _logger.LogWarning("Data file not found at path: {Path}", fullPath);
            return new List<VehicleModelJSON>();
        }

        var jsonData = await System.IO.File.ReadAllTextAsync(fullPath);

        if (string.IsNullOrWhiteSpace(jsonData))
        {
            _logger.LogWarning("Data file is empty: {Path}", fullPath);
            return new List<VehicleModelJSON>();
        }

        var carFeatures = JsonConvert.DeserializeObject<List<VehicleModelJSON>>(jsonData);

        return carFeatures ?? new List<VehicleModelJSON>();
    }
    public async Task<VehicleListResult> GetAllVehiclesByStatusAsync(int pageView, int offset, string? status = null)
    {
        IQueryable<Vehicle> query = _dbContext.Vehicles.OrderBy(v => v.Id);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(v => v.Status == status);
        }
        var totalVehicles = await query.CountAsync();

        var res = await VehicleQuery.GetPaginatedVehiclesAsync(query, offset, pageView);

        return new VehicleListResult
        {
            Vehicles = res,
            TotalCount = totalVehicles
        };
    }
    public async Task<List<string>> GetAllVehicleMakes()
    {
        var makes = await _dbContext.Vehicles
            .Select(v => v.Make)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync();

        return await Task.FromResult(makes);
    }
    public async Task<VehicleListResult> GetVehiclesByMakeAsync(int pageView, int offset, string make)
    {
        var query = _dbContext.Vehicles.AsNoTracking().Where(v => v.Make == make).OrderBy(v => v.Id);

        var totalVehicles = await query.CountAsync();

        var vehicles = await VehicleQuery.GetPaginatedVehiclesAsync(query, offset, pageView);

        return new VehicleListResult
        {
            Vehicles = vehicles,
            TotalCount = totalVehicles
        };
    }
    public async Task<VehicleListResult> GetVehiclesByModelAsync(int pageView, int offset, string model)
    {
        var query = _dbContext.Vehicles.Where(v => v.Model == model).OrderBy(v => v.Id);
        var totalVehicles = await query.CountAsync();

        var result = await VehicleQuery.GetPaginatedVehiclesAsync(query, offset, pageView);

        return new VehicleListResult
        {
            Vehicles = result,
            TotalCount = totalVehicles
        };
    }
    public async Task<List<string>> GetDistinctColorsAsync()
    {
            return await _dbContext.Vehicles
            .Where(v => !string.IsNullOrEmpty(v.Color))
            .Select(v => v.Color!)
            .Distinct()
            .ToListAsync();
    }
    public async Task<List<Vehicle>> SearchVehiclesAsync(VehicleFilterDto filters, int pageView, int offset, string? sortOrder=null)
    {
        var query = _dbContext.Vehicles.AsNoTracking();
        var filteredQuery = VehicleQuery.ApplyFilters(query, filters.Make, filters.Model, filters.StartPrice, filters.EndPrice, filters.Mileage, filters.StartYear, filters.EndYear, filters.Gearbox, filters.SelectedColors, filters.Status);
        query = VehicleQuery.ApplySorting(filteredQuery, sortOrder);
        var vehicles = await VehicleQuery.GetPaginatedVehiclesAsync(query, offset, pageView);

        return vehicles;
    }
    public async Task<int> GetTotalCountAsync(VehicleFilterDto filterDto)
    {
        var query = _dbContext.Vehicles.AsNoTracking();
        var filteredQuery = VehicleQuery.ApplyFilters(query, filterDto.Make, filterDto.Model, filterDto.StartPrice, filterDto.EndPrice, filterDto.Mileage, filterDto.StartYear, filterDto.EndYear, filterDto.Gearbox, filterDto.SelectedColors, filterDto.Status);
        return await filteredQuery.CountAsync();
    }
    public async Task<Dictionary<string, int>> GetGearboxCountsAsync(VehicleFilterDto filterDto)
    {
        var query = _dbContext.Vehicles.AsNoTracking();
        var filteredQuery = VehicleQuery.ApplyFilters(query, filterDto.Make, filterDto.Model, filterDto.StartPrice, filterDto.EndPrice, filterDto.Mileage, filterDto.StartYear, filterDto.EndYear, filterDto.Gearbox, filterDto.SelectedColors, filterDto.Status);
        return await VehicleQuery.GetGearboxCountsAsync(filteredQuery);
    }
    public async Task<Dictionary<string, int>> GetColorsCountsAsync(VehicleFilterDto filterDto)
    {
        var query = _dbContext.Vehicles.AsNoTracking();
        var filteredQuery = VehicleQuery.ApplyFilters(query, filterDto.Make, filterDto.Model, filterDto.StartPrice, filterDto.EndPrice, filterDto.Mileage, filterDto.StartYear, filterDto.EndYear, filterDto.Gearbox, filterDto.SelectedColors, filterDto.Status);
        return await VehicleQuery.GetSelectedColorCounts(filteredQuery);
    }
    public async Task<Vehicle?> GetVehicleByIdAsync(int id)
    {
        return await _dbContext.Vehicles.FindAsync(id);
    }
    public async Task<Vehicle?> GetVehicleByVinAsync(string Vin)
    {
        return await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.Vin == Vin);
    }
    public async Task<Questionnaire> SaveQuestionnaireAsync(QuestionnaireDTO dto)
    {
        var questionnaire = new Questionnaire
        {
            DrivingLicense = dto.DrivingLicense,
            MaritalStatus = dto.MaritalStatus,
            DOB = dto.DOB,
            EmploymentStatus = dto.EmploymentStatus,
            BorrowAmount = dto.BorrowAmount,
            NotSure = dto.NotSure,
            Email = dto.Email,
            Phone = dto.Phone
        };

        _dbContext.Questionnaires.Add(questionnaire);
        await _dbContext.SaveChangesAsync();

        return questionnaire;
    }
    public Task<Vehicle> AddVehicleAsync(Vehicle vehicle)
    {
        _dbContext.Vehicles.Add(vehicle);
        return Task.FromResult(vehicle);
    }
    public Task<bool> UpdateVehicleAsync(Vehicle vehicle)
    {
        _dbContext.Entry(vehicle).State = EntityState.Modified;
        return Task.FromResult(true);
    }
    public async Task<bool> DeleteVehicleAsync(int id)
    {
        var vehicle = await _dbContext.Vehicles.FindAsync(id);
        if (vehicle == null)
            return false;

        _dbContext.Vehicles.Remove(vehicle);
        return true;
    }
    public async Task<List<VehicleOptionsDTO>> GetVehicleOptionsAsync()
    {
        return await _dbContext.Vehicles
            .Select(v => new VehicleOptionsDTO
            {
                Make = v.Make,
                Model = v.Model,
                Year = v.Year
            })
            .Distinct()
            .ToListAsync();
    }
    private bool VehicleExists(int id)
    {
        return _dbContext.Vehicles.Any(v => v.Id == id);
    }
}