using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using AutoFiCore.Utilities;
using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
namespace AutoFiCore.Controllers;

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
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetAllVehiclesByStatus([FromQuery] int pageView, [FromQuery] int offset, [FromQuery] string? status = null)
    {
       
        var validationError = Validator.ValidatePagination(pageView, offset);
        if (validationError != null)
            return BadRequest(validationError);


        var vehicles = await _vehicleService.GetAllVehiclesByStatusAsync(pageView, offset, status);
        return Ok(vehicles);
    }
    [HttpGet("features")]
    public async Task<ActionResult<NormalizedCarFeatureDto>> GetCarFeatures([FromQuery] string make, [FromQuery] string model)
    {
        make = make.Trim();
        var makeValidator = Validator.ValidateMakeOrModel(make);
        if (makeValidator != null)
            return BadRequest(makeValidator);

        model = model.Trim();
        var modelValidator = Validator.ValidateMakeOrModel(model);
        if (modelValidator != null)
            return BadRequest(modelValidator);

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
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehiclesByMake([FromQuery] int pageView, [FromQuery] int offset, [FromQuery] string make)
    {
       
        make = make.Trim();
        var makeValidator = Validator.ValidateMakeOrModel(make);
        if (makeValidator != null)
            return BadRequest(makeValidator);

        var paginationValidator = Validator.ValidatePagination(pageView, offset);
        if (paginationValidator != null)
            return BadRequest(paginationValidator);

        var vehicles = await _vehicleService.GetVehiclesByMakeAsync(pageView, offset, make);
        return Ok(vehicles);
       
    }
    [HttpGet("colors-count")]
    public async Task<ActionResult<Dictionary<string, int>>> GetColorsCount([FromQuery] VehicleFilterDto filters)
    {
        var validationErrors = Validator.ValidateFilters(filters);
        if (validationErrors.Any())
            return BadRequest(string.Join(" ", validationErrors));
        filters = NormalizeInput.NormalizeFilters(filters);
          
        var count = await _vehicleService.GetAvailableColorsCountAsync(filters);
        return Ok(count);
    }

    [HttpGet("gearbox-count")]
    public async Task<ActionResult<Dictionary<string, int>>> GetGearboxCount([FromQuery] VehicleFilterDto filters)
    {
        var validationErrors = Validator.ValidateFilters(filters);
        if (validationErrors.Any())
            return BadRequest(string.Join(" ", validationErrors));
        filters = NormalizeInput.NormalizeFilters(filters);

        var count = await _vehicleService.GetGearboxCountsAsync(filters);
        return Ok(count);
    }
    [HttpGet("total-vehicle-count")]
    public async Task<ActionResult<int>> GetTotalVehicleCount([FromQuery] VehicleFilterDto filters)
    {
        var validationErrors = Validator.ValidateFilters(filters);
        if (validationErrors.Any())
            return BadRequest(string.Join(" ", validationErrors));
        filters = NormalizeInput.NormalizeFilters(filters);

        var count = await _vehicleService.GetTotalCountAsync(filters);
        return Ok(count);
    }

    [HttpGet("search-vehicles")]
    public async Task<ActionResult<IEnumerable<Vehicle>>> SearchVehicles([FromQuery] VehicleFilterDto filters, [FromQuery] int pageView, [FromQuery] int offset, [FromQuery] string? sortOrder = null)
    {
        var validationErrors = Validator.ValidateFilters(filters);
        if (validationErrors.Any())
            return BadRequest(string.Join(" ", validationErrors));

        var paginationValidator = Validator.ValidatePagination(pageView, offset);
        if (paginationValidator != null)
            return BadRequest(paginationValidator);

        filters = NormalizeInput.NormalizeFilters(filters);

        var vehicles = await _vehicleService.SearchVehiclesAsync(filters, pageView, offset, sortOrder);
        return Ok(vehicles);
    }

    [HttpGet("by-model")]
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehiclesByModel([FromQuery] int pageView, [FromQuery] int offset, [FromQuery] string model)
    {
        model = model.Trim();
        var paginationValidator = Validator.ValidatePagination(pageView, offset);

        if (paginationValidator != null)
            return BadRequest(paginationValidator);

        var modelValidator = Validator.ValidateMakeOrModel(model);
        if (modelValidator != null)
            return BadRequest(modelValidator);

        var vehicles = await _vehicleService.GetVehiclesByModelAsync(pageView, offset, model);
        return Ok(vehicles);
    }

    [HttpGet("get-makes")]
    public async Task<ActionResult<List<string>>> GetAllMakes() 
    {
        var makes = await _vehicleService.GetAllVehiclesMakesAsync();
        return Ok(makes);
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
