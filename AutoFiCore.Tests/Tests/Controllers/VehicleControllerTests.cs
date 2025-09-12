using AutoFiCore.Controllers;
using AutoFiCore.Dto;
using AutoFiCore.DTOs;
using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Controllers
{
    public class VehicleControllerTests
    {
        private readonly Mock<IVehicleService> _vehicleServiceMock;
        private readonly Mock<ILoanService> _loanServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IPdfService> _pdfServiceMock;
        private readonly Mock<ILogger<VehicleController>> _loggerMock;
        private readonly VehicleController _controller;

        public VehicleControllerTests()
        {
            _vehicleServiceMock = new Mock<IVehicleService>();
            _loanServiceMock = new Mock<ILoanService>();
            _emailServiceMock = new Mock<IEmailService>();
            _pdfServiceMock = new Mock<IPdfService>();
            _loggerMock = new Mock<ILogger<VehicleController>>();
            _controller = new VehicleController(
                _vehicleServiceMock.Object,
                _loggerMock.Object,
                _loanServiceMock.Object,
                _emailServiceMock.Object,
                _pdfServiceMock.Object
            );
        }

        [Fact]
        public async Task GetAllVehiclesByStatus_ReturnsOk()
        {
            var pagination = new PaginationParams { PageView = 10, Offset = 0 };
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, Vin = "12345678901234567", Make = "A", Model = "B" }
            };

            var vehicleListResult = new VehicleListResult
            {
                Vehicles = vehicles,
                TotalCount = vehicles.Count
            };

            _vehicleServiceMock
                .Setup(s => s.GetAllVehiclesByStatusAsync(10, 0, null))
                .ReturnsAsync(vehicleListResult);

            var response = await _controller.GetAllVehiclesByStatus(pagination, null);

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            Assert.Equal(vehicleListResult, ok.Value);
        }

        [Fact]
        public async Task GetCarFeatures_ReturnsNotFound_WhenNoFeatures()
        {
            _vehicleServiceMock.Setup(s => s.GetAllCarFeaturesAsync()).ReturnsAsync(new List<VehicleModelJSON>());

            var result = await _controller.GetCarFeatures("make", "model");

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("No car features found", notFound.Value.ToString());
        }

        [Fact]
        public async Task GetCarFeatures_ReturnsNotFound_WhenNoMatch()
        {
            _vehicleServiceMock.Setup(s => s.GetAllCarFeaturesAsync()).ReturnsAsync(new List<VehicleModelJSON> { new VehicleModelJSON() });
            _vehicleServiceMock.Setup(s => s.GetCarFeature(It.IsAny<List<VehicleModelJSON>>(), "make", "model")).Returns((VehicleModelJSON)null);

            var result = await _controller.GetCarFeatures("make", "model");

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("No data found", notFound.Value.ToString());
        }

        [Fact]
        public async Task GetCarFeatures_ReturnsOk_WhenFound()
        {
            var features = new VehicleModelJSON();
            _vehicleServiceMock.Setup(s => s.GetAllCarFeaturesAsync()).ReturnsAsync(new List<VehicleModelJSON> { features });
            _vehicleServiceMock.Setup(s => s.GetCarFeature(It.IsAny<List<VehicleModelJSON>>(), "make", "model")).Returns(features);

            var result = await _controller.GetCarFeatures("make", "model");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<NormalizedCarFeatureDto>(ok.Value);
        }

        [Fact]
        public async Task GetAllCarColors_ReturnsOk()
        {
            var colors = new List<string> { "Red", "Blue" };
            _vehicleServiceMock.Setup(s => s.GetDistinctColorsAsync()).ReturnsAsync(colors);

            var result = await _controller.GetAllCarColors();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(colors, ok.Value);
        }

        [Fact]
        public async Task GetVehiclesByMake_ReturnsOk()
        {
            var pagination = new PaginationParams { PageView = 10, Offset = 0 };
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, Vin = "12345678901234567", Make = "A", Model = "B" }
            };

            var vehicleListResult = new VehicleListResult
            {
                Vehicles = vehicles,
                TotalCount = vehicles.Count
            };

            _vehicleServiceMock
                .Setup(s => s.GetVehiclesByMakeAsync(10, 0, "A"))
                .ReturnsAsync(vehicleListResult);

            var response = await _controller.GetVehiclesByMake(pagination, "A");

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            Assert.Equal(vehicleListResult, ok.Value);
        }

        [Fact]
        public async Task GetVehicleOptions_ReturnsOk()
        {
            var options = new List<VehicleOptionsDTO> { new VehicleOptionsDTO() };
            _vehicleServiceMock.Setup(s => s.GetVehicleOptionsAsync()).ReturnsAsync(options);

            var result = await _controller.GetVehicleOptions();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(options, ok.Value);
        }

        [Fact]
        public async Task GetColorsCount_ReturnsOk()
        {
            var filters = new VehicleFilterDto();
            var dict = new Dictionary<string, int> { { "Red", 2 } };
            _vehicleServiceMock.Setup(s => s.GetAvailableColorsCountAsync(It.IsAny<VehicleFilterDto>())).ReturnsAsync(dict);

            var result = await _controller.GetColorsCount(filters);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(dict, ok.Value);
        }

        [Fact]
        public async Task GetGearboxCount_ReturnsOk()
        {
            var filters = new VehicleFilterDto();
            var dict = new Dictionary<string, int> { { "Auto", 3 } };
            _vehicleServiceMock.Setup(s => s.GetGearboxCountsAsync(It.IsAny<VehicleFilterDto>())).ReturnsAsync(dict);

            var result = await _controller.GetGearboxCount(filters);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(dict, ok.Value);
        }

        [Fact]
        public async Task GetTotalVehicleCount_ReturnsOk()
        {
            var filters = new VehicleFilterDto();
            _vehicleServiceMock.Setup(s => s.GetTotalCountAsync(It.IsAny<VehicleFilterDto>())).ReturnsAsync(5);

            var result = await _controller.GetTotalVehicleCount(filters);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(5, ok.Value);
        }

        [Fact]
        public async Task SearchVehicles_ReturnsOk()
        {
            var filters = new VehicleFilterDto();
            var pagination = new PaginationParams { PageView = 10, Offset = 0 };
            var vehicles = new List<Vehicle> { new Vehicle { Id = 1, Vin = "12345678901234567", Make = "A", Model = "B" } };
            _vehicleServiceMock.Setup(s => s.SearchVehiclesAsync(It.IsAny<VehicleFilterDto>(), 10, 0, null)).ReturnsAsync(vehicles);

            var result = await _controller.SearchVehicles(filters, pagination, null);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(vehicles, ok.Value);
        }


        [Fact]
        public async Task GetVehiclesByModel_ReturnsOk()
        {
            var pagination = new PaginationParams { PageView = 10, Offset = 0 };
            var vehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, Vin = "12345678901234567", Make = "A", Model = "B" }
            };

            var vehicleListResult = new VehicleListResult
            {
                Vehicles = vehicles,
                TotalCount = vehicles.Count
            };

            _vehicleServiceMock
                .Setup(s => s.GetVehiclesByModelAsync(10, 0, "B"))
                .ReturnsAsync(vehicleListResult);

            var response = await _controller.GetVehiclesByModel(pagination, "B");

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            Assert.Equal(vehicleListResult, ok.Value);
        }
        [Fact]
        public async Task GetAllMakes_ReturnsOk()
        {
            var makes = new List<string> { "A", "B" };
            _vehicleServiceMock.Setup(s => s.GetAllVehiclesMakesAsync()).ReturnsAsync(makes);

            var result = await _controller.GetAllMakes();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(makes, ok.Value);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsOk()
        {
            var categories = new List<string> { "SUV", "Sedan" };
            _vehicleServiceMock.Setup(s => s.GetAllVehiclesCategoriesAsync()).ReturnsAsync(categories);

            var result = await _controller.GetAllCategories();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(categories, ok.Value);
        }

        [Fact]
        public async Task GetVehicleById_ReturnsNotFound_WhenNull()
        {
            _vehicleServiceMock.Setup(s => s.GetVehicleByIdAsync(1)).ReturnsAsync((Vehicle)null);

            var result = await _controller.GetVehicleById(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task GetVehicleById_ReturnsOk_WhenFound()
        {
            var vehicle = new Vehicle { Id = 1, Vin = "12345678901234567", Make = "A", Model = "B" };
            _vehicleServiceMock.Setup(s => s.GetVehicleByIdAsync(1)).ReturnsAsync(vehicle);

            var result = await _controller.GetVehicleById(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(vehicle, ok.Value);
        }

        [Fact]
        public async Task GetVehicleByVin_ReturnsNotFound_WhenNull()
        {
            _vehicleServiceMock.Setup(s => s.GetVehicleByVinAsync("vin")).ReturnsAsync((Vehicle)null);

            var result = await _controller.GetVehicleByVin("vin");

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task GetVehicleByVin_ReturnsOk_WhenFound()
        {
            var vehicle = new Vehicle { Id = 1, Vin = "12345678901234567", Make = "A", Model = "B" };
            _vehicleServiceMock.Setup(s => s.GetVehicleByVinAsync("vin")).ReturnsAsync(vehicle);

            var result = await _controller.GetVehicleByVin("vin");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(vehicle, ok.Value);
        }

        [Fact]
        public async Task SaveQuestionnaire_ReturnsOk()
        {
            var dto = new QuestionnaireDTO { Email = "test@test.com", BorrowAmount = 10000, DrivingLicense = "DL", MaritalStatus = "Single", DOB = DateOnly.FromDateTime(DateTime.Now), EmploymentStatus = "Employed", Phone = "123" };
            var questionnaire = new Questionnaire();
            var loan = new LoanCalculation();
            var pdf = new byte[] { 1, 2, 3 };
            _vehicleServiceMock.Setup(s => s.SaveQuestionnaireAsync(dto)).ReturnsAsync(questionnaire);
            _loanServiceMock.Setup(s => s.CalculateLoanAsync(It.IsAny<LoanRequest>())).ReturnsAsync(loan);
            _pdfServiceMock.Setup(s => s.GenerateLoanPdf(questionnaire, loan)).Returns(pdf);
            _emailServiceMock.Setup(s => s.SendLoanEmailAsync(dto.Email, pdf)).Returns(Task.CompletedTask);

            var result = await _controller.SaveQuestionnaire(dto, 1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<SaveQuestionnaireResponse>(ok.Value);
            Assert.Equal(questionnaire, response.Questionnaire);
            Assert.Equal(loan, response.Loan);
        }

        [Fact]
        public async Task CreateVehicle_ReturnsBadRequest_WhenExists()
        {
            var vehicle = new Vehicle { Vin = "12345678901234567", Make = "A", Model = "B" };
            _vehicleServiceMock.Setup(s => s.GetVehicleByVinAsync(vehicle.Vin)).ReturnsAsync(vehicle);

            var result = await _controller.CreateVehicle(vehicle);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("already exists", bad.Value.ToString());
        }

        [Fact]
        public async Task CreateVehicle_ReturnsCreated_WhenSuccess()
        {
            var vehicle = new Vehicle { Id = 1, Vin = "12345678901234567", Make = "A", Model = "B" };
            _vehicleServiceMock.Setup(s => s.GetVehicleByVinAsync(vehicle.Vin)).ReturnsAsync((Vehicle)null);
            _vehicleServiceMock.Setup(s => s.CreateVehicleAsync(vehicle)).ReturnsAsync(vehicle);

            var result = await _controller.CreateVehicle(vehicle);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(vehicle, created.Value);
        }

        [Fact]
        public async Task UpdateVehicle_ReturnsBadRequest_WhenIdMismatch()
        {
            var vehicle = new Vehicle { Id = 2, Vin = "12345678901234567", Make = "A", Model = "B" };

            var result = await _controller.UpdateVehicle(1, vehicle);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("ID mismatch", bad.Value.ToString());
        }

        [Fact]
        public async Task UpdateVehicle_ReturnsNotFound_WhenNotFound()
        {
            var vehicle = new Vehicle { Id = 1, Vin = "12345678901234567", Make = "A", Model = "B" };
            _vehicleServiceMock.Setup(s => s.GetVehicleByIdAsync(1)).ReturnsAsync((Vehicle)null);

            var result = await _controller.UpdateVehicle(1, vehicle);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task UpdateVehicle_ReturnsOk_WhenSuccess()
        {
            var vehicle = new Vehicle { Id = 1, Vin = "12345678901234567", Make = "A", Model = "B" };
            _vehicleServiceMock.Setup(s => s.GetVehicleByIdAsync(1)).ReturnsAsync(vehicle);
            _vehicleServiceMock.Setup(s => s.UpdateVehicleAsync(vehicle)).ReturnsAsync(vehicle);

            var result = await _controller.UpdateVehicle(1, vehicle);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(vehicle, ok.Value);
        }

        [Fact]
        public async Task DeleteVehicle_ReturnsNotFound_WhenNotFound()
        {
            _vehicleServiceMock.Setup(s => s.DeleteVehicleAsync(1)).ReturnsAsync(false);

            var result = await _controller.DeleteVehicle(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task DeleteVehicle_ReturnsNoContent_WhenSuccess()
        {
            _vehicleServiceMock.Setup(s => s.DeleteVehicleAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteVehicle(1);

            Assert.IsType<NoContentResult>(result);
        }
    }
}