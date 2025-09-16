using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.DTOs;
using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.Json;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Controller for managing vehicle-related operations such as creating, updating, retrieving, and deleting vehicles,
    /// as well as features, colors, and questionnaires.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILoanService _loanService;
        private readonly IEmailService _emailService;
        private readonly IPdfService _pdfService;
        private readonly ILogger<VehicleController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleController"/> class.
        /// </summary>
        /// <param name="vehicleService">Service for vehicle operations.</param>
        /// <param name="logger">Logger for the controller.</param>
        /// <param name="loanService">Service for loan calculations.</param>
        /// <param name="emailService">Service for sending emails.</param>
        /// <param name="pdfService">Service for generating PDFs.</param>
        public VehicleController(IVehicleService vehicleService, ILogger<VehicleController> logger, ILoanService loanService, IEmailService emailService, IPdfService pdfService)
        {
            _vehicleService = vehicleService;
            _logger = logger;
            _loanService = loanService;
            _emailService = emailService;
            _pdfService = pdfService;
        }

        /// <summary>
        /// Retrieves all vehicles optionally filtered by status with pagination.
        /// </summary>
        /// <param name="paginationParams">Pagination parameters (page size and offset).</param>
        /// <param name="status">Optional vehicle status to filter by.</param>
        /// <returns>Returns a list of vehicles.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet]
        public async Task<IActionResult> GetAllVehiclesByStatus([FromQuery] PaginationParams paginationParams, [FromQuery] string? status = null)
        {
            var vehicles = await _vehicleService.GetAllVehiclesByStatusAsync(
                paginationParams.PageView,
                paginationParams.Offset,
                status);
            return Ok(vehicles.Value);
        }

        /// <summary>
        /// Retrieves all normalized features for a specific car make and model.
        /// </summary>
        /// <param name="make">Car make.</param>
        /// <param name="model">Car model.</param>
        /// <returns>Returns the normalized car features or NotFound if not available.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("features")]
        public async Task<IActionResult> GetCarFeatures([FromQuery] string make, [FromQuery] string model)
        {
            var carFeatures = await _vehicleService.GetAllCarFeaturesAsync();
            if (carFeatures == null || carFeatures.Value!.Count == 0)
                return NotFound("No car features found.");

            var match = _vehicleService.GetCarFeature(carFeatures.Value, make, model);

            if (match == null)
                return NotFound($"No data found for {make} {model}.");

            var normalized = NormalizeInput.NormalizeCarFeatures(match.Value!);
            return Ok(normalized);
        }

        /// <summary>
        /// Retrieves all distinct car colors.
        /// </summary>
        /// <returns>Returns a list of car colors.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("get-colors")]
        public async Task<IActionResult> GetAllCarColors()
        {
            var result = await _vehicleService.GetDistinctColorsAsync();
            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves vehicles filtered by a specific make with pagination.
        /// </summary>
        /// <param name="paginationParams">Pagination parameters.</param>
        /// <param name="make">Vehicle make to filter by.</param>
        /// <returns>Returns a list of vehicles matching the make.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("by-make")]
        public async Task<IActionResult> GetVehiclesByMake([FromQuery] PaginationParams paginationParams, [FromQuery] string make)
        {
            var vehicles = await _vehicleService.GetVehiclesByMakeAsync(
                paginationParams.PageView,
                paginationParams.Offset,
                make);
            return Ok(vehicles.Value);
        }

        /// <summary>
        /// Retrieves all available vehicle options.
        /// </summary>
        /// <returns>Returns a list of vehicle options.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("get-vehicle-options")]
        public async Task<IActionResult> GetVehicleOptions()
        {
            var vehicleOptions = await _vehicleService.GetVehicleOptionsAsync();
            return Ok(vehicleOptions.Value);
        }

        /// <summary>
        /// Retrieves the count of colors for vehicles matching specified filters.
        /// </summary>
        /// <param name="filters">Filter criteria.</param>
        /// <returns>Returns a dictionary with color counts.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("colors-count")]
        public async Task<IActionResult> GetColorsCount([FromQuery] VehicleFilterDto filters)
        {
            filters = NormalizeInput.NormalizeFilters(filters);
            var count = await _vehicleService.GetAvailableColorsCountAsync(filters);
            return Ok(count.Value);
        }

        /// <summary>
        /// Retrieves the count of gearbox types for vehicles matching specified filters.
        /// </summary>
        /// <param name="filters">Filter criteria.</param>
        /// <returns>Returns a dictionary with gearbox type counts.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("gearbox-count")]
        public async Task<IActionResult> GetGearboxCount([FromQuery] VehicleFilterDto filters)
        {
            filters = NormalizeInput.NormalizeFilters(filters);
            var count = await _vehicleService.GetGearboxCountsAsync(filters);
            return Ok(count.Value);
        }

        /// <summary>
        /// Retrieves the total number of vehicles matching specified filters.
        /// </summary>
        /// <param name="filters">Filter criteria.</param>
        /// <returns>Returns the total count of vehicles.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("total-vehicle-count")]
        public async Task<IActionResult> GetTotalVehicleCount([FromQuery] VehicleFilterDto filters)
        {
            filters = NormalizeInput.NormalizeFilters(filters);
            var count = await _vehicleService.GetTotalCountAsync(filters);
            return Ok(count.Value);
        }

        /// <summary>
        /// Searches vehicles using filter criteria with pagination and optional sorting.
        /// </summary>
        /// <param name="filters">Vehicle filters.</param>
        /// <param name="paginationParams">Pagination parameters.</param>
        /// <param name="sortOrder">Optional sort order.</param>
        /// <returns>Returns a list of vehicles matching the search criteria.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("search-vehicles")]
        public async Task<IActionResult> SearchVehicles([FromQuery] VehicleFilterDto filters, [FromQuery] PaginationParams paginationParams, [FromQuery] string? sortOrder = null)
        {
            filters = NormalizeInput.NormalizeFilters(filters);
            var vehicles = await _vehicleService.SearchVehiclesAsync(
                filters,
                paginationParams.PageView,
                paginationParams.Offset,
                sortOrder);
            return Ok(vehicles.Value);
        }

        /// <summary>
        /// Retrieves vehicles filtered by a specific model with pagination.
        /// </summary>
        /// <param name="paginationParams">Pagination parameters.</param>
        /// <param name="model">Vehicle model.</param>
        /// <returns>Returns a list of vehicles matching the model.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("by-model")]
        public async Task<IActionResult> GetVehiclesByModel([FromQuery] PaginationParams paginationParams, [FromQuery] string model)
        {
            var vehicles = await _vehicleService.GetVehiclesByModelAsync(
                paginationParams.PageView,
                paginationParams.Offset,
                model);
            return Ok(vehicles.Value);
        }

        /// <summary>
        /// Retrieves all distinct vehicle makes.
        /// </summary>
        /// <returns>Returns a list of vehicle makes.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("get-makes")]
        public async Task<IActionResult> GetAllMakes()
        {
            var makes = await _vehicleService.GetAllVehiclesMakesAsync();
            return Ok(makes.Value);
        }

        /// <summary>
        /// Retrieves all distinct vehicle categories.
        /// </summary>
        /// <returns>Returns a list of vehicle categories.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("get-categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _vehicleService.GetAllVehiclesCategoriesAsync();
            return Ok(categories.Value);
        }

        /// <summary>
        /// Retrieves a vehicle by its ID.
        /// </summary>
        /// <param name="id">Vehicle ID.</param>
        /// <returns>Returns the vehicle or NotFound if not found.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null)
                return NotFound($"Vehicle with ID {id} not found");
            return Ok(vehicle.Value);
        }

        /// <summary>
        /// Retrieves a vehicle by its VIN.
        /// </summary>
        /// <param name="vin">Vehicle VIN.</param>
        /// <returns>Returns the vehicle or NotFound if not found.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("vin/{vin}")]
        public async Task<IActionResult> GetVehicleByVin(string vin)
        {
            var vehicle = await _vehicleService.GetVehicleByVinAsync(vin);
            if (vehicle == null)
                return NotFound($"Vehicle with VIN {vin} not found");
            return Ok(vehicle.Value);
        }

        /// <summary>
        /// Saves a questionnaire and generates a loan PDF for the specified vehicle.
        /// </summary>
        /// <param name="dto">Questionnaire details.</param>
        /// <param name="vehicleId">Vehicle ID associated with the questionnaire.</param>
        /// <returns>Returns the saved questionnaire and loan details.</returns>
        [AllowAnonymous]
        [HttpPost("save-questionnaire")]
        public async Task<IActionResult> SaveQuestionnaire([FromBody] QuestionnaireDTO dto, [FromQuery] int vehicleId)
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
            var pdfBytes = _pdfService.GenerateLoanPdf(questionnaire.Value!, loanDetails.Value!);
            await _emailService.SendLoanEmailAsync(questionnaire.Value!.Email, pdfBytes);

            return Ok(new SaveQuestionnaireResponse
            {
                Questionnaire = questionnaire.Value!,
                Loan = loanDetails.Value!
            });
        }

        /// <summary>
        /// Creates a new vehicle.
        /// </summary>
        /// <param name="vehicle">Vehicle object to create.</param>
        /// <returns>Returns the created vehicle.</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateVehicle(Vehicle vehicle)
        {
            var existingVehicle = await _vehicleService.GetVehicleByVinAsync(vehicle.Vin);
            if (existingVehicle != null)
                return BadRequest($"Vehicle with VIN {vehicle.Vin} already exists");

            var createdVehicle = await _vehicleService.CreateVehicleAsync(vehicle);
            return CreatedAtAction(nameof(GetVehicleById), new { id = createdVehicle.Value!.Id }, createdVehicle);
        }

        /// <summary>
        /// Updates an existing vehicle.
        /// </summary>
        /// <param name="id">Vehicle ID to update.</param>
        /// <param name="vehicle">Vehicle object with updated data.</param>
        /// <returns>Returns the updated vehicle.</returns>
        [AllowAnonymous]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, Vehicle vehicle)
        {
            if (id != vehicle.Id)
                return BadRequest("ID mismatch");

            var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (existingVehicle == null)
                return NotFound($"Vehicle with ID {id} not found");

            var updatedVehicle = await _vehicleService.UpdateVehicleAsync(vehicle);
            return Ok(updatedVehicle.Value);
        }

        /// <summary>
        /// Deletes a vehicle by its ID.
        /// </summary>
        /// <param name="id">Vehicle ID to delete.</param>
        /// <returns>Returns NoContent if deleted or NotFound if vehicle does not exist.</returns>
        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var result = await _vehicleService.DeleteVehicleAsync(id);
            if (!result.IsSuccess)                
                return NotFound($"Vehicle with ID {id} not found");
            return NoContent();
        }
    }
}
