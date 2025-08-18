using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using AutoFiCore.Utilities;
using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
namespace AutoFiCore.Controllers;

using Utilities;
using AutoFiCore.Dto;
using AutoFiCore.DTOs;
using Newtonsoft.Json;
[ApiController]
[Route("[controller]")]

public class VehicleController : ControllerBase
{
    private readonly IVehicleService _vehicleService;
    private readonly ILoanService _loanService;
    private readonly IEmailService _emailService;
    private readonly IPdfService _pdfService;
    private readonly ILogger<VehicleController> _logger;

    public VehicleController(IVehicleService vehicleService, ILogger<VehicleController> logger, ILoanService loanService, IEmailService emailService, IPdfService pdfService)
    {
        _vehicleService = vehicleService;
        _logger = logger;
        _loanService = loanService;
        _emailService = emailService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetAllVehiclesByStatus([FromQuery] PaginationParams paginationParams, [FromQuery] string? status = null)
    {
        var vehicles = await _vehicleService.GetAllVehiclesByStatusAsync(paginationParams.PageView, paginationParams.Offset, status);
        return Ok(vehicles);
    }
    [HttpGet("features")]
    public async Task<ActionResult<NormalizedCarFeatureDto>> GetCarFeatures([FromQuery] string make, [FromQuery] string model)
    {
        var carFeatures = await _vehicleService.GetAllCarFeaturesAsync();
        if (carFeatures == null || carFeatures.Count == 0)
            return NotFound("No car features found.");

        var match = _vehicleService.GetCarFeature(carFeatures, make, model);

        if (match == null)
            return NotFound($"No data found for {make} {model}.");

        var normalized = NormalizeInput.NormalizeCarFeatures(match);

        return Ok(normalized);
    }
    [HttpGet("get-colors")]
    public async Task<ActionResult<List<string>>> GetAllCarColors()
    {

        var result = await _vehicleService.GetDistinctColorsAsync();
        return Ok(result);
    }

    [HttpGet("by-make")]
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehiclesByMake([FromQuery] PaginationParams paginationParams, [FromQuery] string make)
    {
        var vehicles = await _vehicleService.GetVehiclesByMakeAsync(paginationParams.PageView, paginationParams.Offset, make);
        return Ok(vehicles);
    }

    [HttpGet("get-vehicle-options")]
    public async Task<ActionResult<List<VehicleOptionsDTO>>> GetVehicleOptions()
    {
        var vehicleOptions = await _vehicleService.GetVehicleOptionsAsync();
        return Ok(vehicleOptions);
    }

    [HttpGet("colors-count")]
    public async Task<ActionResult<Dictionary<string, int>>> GetColorsCount([FromQuery] VehicleFilterDto filters)
    {
        filters = NormalizeInput.NormalizeFilters(filters);

        var count = await _vehicleService.GetAvailableColorsCountAsync(filters);
        return Ok(count);
    }

    [HttpGet("gearbox-count")]
    public async Task<ActionResult<Dictionary<string, int>>> GetGearboxCount([FromQuery] VehicleFilterDto filters)
    {
        filters = NormalizeInput.NormalizeFilters(filters);

        var count = await _vehicleService.GetGearboxCountsAsync(filters);
        return Ok(count);
    }
    [HttpGet("total-vehicle-count")]
    public async Task<ActionResult<int>> GetTotalVehicleCount([FromQuery] VehicleFilterDto filters)
    {
        filters = NormalizeInput.NormalizeFilters(filters);

        var count = await _vehicleService.GetTotalCountAsync(filters);
        return Ok(count);
    }

    [HttpGet("search-vehicles")]
    public async Task<ActionResult<IEnumerable<Vehicle>>> SearchVehicles([FromQuery] VehicleFilterDto filters, [FromQuery] PaginationParams paginationParams, [FromQuery] string? sortOrder = null)
    {
        filters = NormalizeInput.NormalizeFilters(filters);
        var vehicles = await _vehicleService.SearchVehiclesAsync(filters, paginationParams.PageView, paginationParams.Offset, sortOrder);
        return Ok(vehicles);
    }

    [HttpGet("by-model")]
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehiclesByModel([FromQuery] PaginationParams paginationParams, [FromQuery] string model)
    {
        var vehicles = await _vehicleService.GetVehiclesByModelAsync(paginationParams.PageView, paginationParams.Offset, model);
        return Ok(vehicles);
    }

    [HttpGet("get-makes")]
    public async Task<ActionResult<List<string>>> GetAllMakes()
    {
        var makes = await _vehicleService.GetAllVehiclesMakesAsync();
        return Ok(makes);
    }

    [HttpGet("get-categories")]
    public async Task<ActionResult<List<string>>> GetAllCategories()
    {
        var categories = await _vehicleService.GetAllVehiclesCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Vehicle>> GetVehicleById(int id)
    {
        var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
        if (vehicle == null)
        {
            return NotFound($"Vehicle with ID {id} not found");
        }
        return Ok(vehicle);
    }

    [HttpGet("vin/{vin}")]
    public async Task<ActionResult<Vehicle>> GetVehicleByVin(string vin)
    {
        var vehicle = await _vehicleService.GetVehicleByVinAsync(vin);
        if (vehicle == null)
        {
            return NotFound($"Vehicle with VIN {vin} not found");
        }
        return Ok(vehicle);
    }

    [HttpPost("save-questionnaire")]
    public async Task<ActionResult<object>> SaveQuestionnaire([FromBody] QuestionnaireDTO dto, [FromQuery] int vehicleId)
    {
        var questionnaire = await _vehicleService.SaveQuestionnaireAsync(dto);

        var loanRequest = new LoanRequest
        {
            VehicleId = vehicleId,
            LoanAmount = dto.BorrowAmount,
            InterestRate = 7.5m,
            LoanTermMonths = 60
        };

        var loanDetails = await _loanService.CalculateLoanAsync(loanRequest);

        var pdfBytes = _pdfService.GenerateLoanPdf(questionnaire, loanDetails);
        await _emailService.SendLoanEmailAsync(questionnaire.Email, pdfBytes);

        return Ok(new SaveQuestionnaireResponse
        {
            Questionnaire = questionnaire,
            Loan = loanDetails
        });
    }
    [HttpPost]
    public async Task<ActionResult<Vehicle>> CreateVehicle(Vehicle vehicle)
    {
        var existingVehicle = await _vehicleService.GetVehicleByVinAsync(vehicle.Vin);
        if (existingVehicle != null)
        {
            return BadRequest($"Vehicle with VIN {vehicle.Vin} already exists");
        }

        var createdVehicle = await _vehicleService.CreateVehicleAsync(vehicle);
        return CreatedAtAction(nameof(GetVehicleById), new { id = createdVehicle!.Id }, createdVehicle);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Vehicle>> UpdateVehicle(int id, Vehicle vehicle)
    {
        if (id != vehicle.Id)
        {
            return BadRequest("ID mismatch");
        }

        var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
        if (existingVehicle == null)
        {
            return NotFound($"Vehicle with ID {id} not found");
        }

        var updatedVehicle = await _vehicleService.UpdateVehicleAsync(vehicle);
        return Ok(updatedVehicle);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteVehicle(int id)
    {
        var result = await _vehicleService.DeleteVehicleAsync(id);
        if (!result)
        {
            return NotFound($"Vehicle with ID {id} not found");
        }
        return NoContent();
    }
}
