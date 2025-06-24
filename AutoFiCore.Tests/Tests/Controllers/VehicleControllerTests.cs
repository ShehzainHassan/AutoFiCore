using Xunit;
using Moq;
using AutoFiCore.Controllers;
using AutoFiCore.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using AutoFiCore.Models;
using AutoFiCore.Dto;
using AutoFiCore.DTOs;

namespace AutoFiCore.Tests.Controllers
{
    public class VehicleControllerTests
    {
        private readonly Mock<IVehicleService> _mockVehicleService;
        private readonly Mock<ILogger<VehicleController>> _mockLogger;
        private readonly Mock<ILoanService> _mockLoanService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IPdfService> _mockPdfService;

        private readonly VehicleController _controller;

        public VehicleControllerTests()
        {
            _mockVehicleService = new Mock<IVehicleService>();
            _mockLogger = new Mock<ILogger<VehicleController>>();
            _mockLoanService = new Mock<ILoanService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockPdfService = new Mock<IPdfService>();

            _controller = new VehicleController(
                _mockVehicleService.Object,
                _mockLogger.Object,
                _mockLoanService.Object,
                _mockEmailService.Object,
                _mockPdfService.Object
            );
        }

        [Fact]
        public async Task GetAllMakes_ReturnsOk_WithExpectedMakes()
        {
            var expectedMakes = new List<string> { "BMW", "Mercedes", "Audi" };

            _mockVehicleService
                .Setup(s => s.GetAllVehiclesMakesAsync())
                .ReturnsAsync(expectedMakes);

            var result = await _controller.GetAllMakes();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualMakes = Assert.IsAssignableFrom<List<string>>(okResult.Value);

            Assert.Equal(expectedMakes, actualMakes);

            _mockVehicleService.Verify(s => s.GetAllVehiclesMakesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllCarColors_ReturnsOk_WithExpectedColors()
        {
            var expectedColors = new List<string> { "Red", "Blue", "Green" };

            _mockVehicleService
                .Setup(s => s.GetDistinctColorsAsync())
                .ReturnsAsync(expectedColors);

            var result = await _controller.GetAllCarColors();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualColors = Assert.IsAssignableFrom<List<string>>(okResult.Value);

            Assert.Equal(expectedColors, actualColors);

            _mockVehicleService.Verify(s => s.GetDistinctColorsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetVehiclesByMake_ValidMake_ReturnsOk()
        {
            var expectedResult = new VehicleListResult
            {
                Vehicles = new List<Vehicle>
                {
                    new Vehicle { Id = 1, Make = "BMW", Model = "X5" }
                },
                TotalCount = 1
            };

            _mockVehicleService
                .Setup(s => s.GetVehiclesByMakeAsync(10, 0, "BMW"))
                .ReturnsAsync(expectedResult);

            var result = await _controller.GetVehiclesByMake(10, 0, "BMW");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualResult = Assert.IsAssignableFrom<VehicleListResult>(okResult.Value);

            Assert.Equal(expectedResult.TotalCount, actualResult.TotalCount);
            Assert.Equal(expectedResult.Vehicles.Count(), actualResult.Vehicles.Count());

            _mockVehicleService.Verify(s => s.GetVehiclesByMakeAsync(10, 0, "BMW"), Times.Once);
        }

        [Fact]
        public async Task GetVehiclesByMake_InvalidMake_ReturnsBadRequest()
        {
            var result = await _controller.GetVehiclesByMake(10, 0, "");

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetColorsCount_ValidFilters_ReturnsOk()
        {
            var filters = new VehicleFilterDto { Make = "BMW" };
            var expectedCounts = new Dictionary<string, int>
            {
                { "Red", 3 },
                { "Blue", 2 }
            };

            _mockVehicleService
                .Setup(s => s.GetAvailableColorsCountAsync(filters))
                .ReturnsAsync(expectedCounts);

            var result = await _controller.GetColorsCount(filters);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualCounts = Assert.IsAssignableFrom<Dictionary<string, int>>(okResult.Value);

            Assert.Equal(expectedCounts, actualCounts);

            _mockVehicleService.Verify(s => s.GetAvailableColorsCountAsync(filters), Times.Once);
        }

        [Fact]
        public async Task GetColorsCount_InvalidFilters_ReturnsBadRequest()
        {
            var filters = new VehicleFilterDto
            {
                Make = "",
                Model = "",
                StartPrice = -100,
                EndPrice = -200,
                Mileage = -10,
                StartYear = 2025,
                EndYear = 2000
            };

            var result = await _controller.GetColorsCount(filters);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetGearboxCount_ValidFilters_ReturnsOkWithCounts()
        {
            var filters = new VehicleFilterDto { Make = "BMW" };
            var expectedCounts = new Dictionary<string, int>
            {
                { "Automatic", 10 },
                { "Manual", 5 }
            };

            _mockVehicleService
                .Setup(s => s.GetGearboxCountsAsync(filters))
                .ReturnsAsync(expectedCounts);

            var result = await _controller.GetGearboxCount(filters);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualCounts = Assert.IsAssignableFrom<Dictionary<string, int>>(okResult.Value);

            Assert.Equal(expectedCounts, actualCounts);

            _mockVehicleService.Verify(s => s.GetGearboxCountsAsync(filters), Times.Once);
        }

        [Fact]
        public async Task GetGearboxCount_InvalidFilters_ReturnsBadRequest()
        {
            var filters = new VehicleFilterDto
            {
                Make = "",
                Model = "",
                StartPrice = -40000,
                EndPrice = -24000,
                Mileage = -10,
                StartYear = 3000,   
                EndYear = 1000,    
                Gearbox = "",
                Status = ""
            };

            var result = await _controller.GetGearboxCount(filters);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetTotalVehicleCount_ValidFilters_ReturnsOk()
        {
            var filters = new VehicleFilterDto { Make = "Audi" };
            var expectedCount = 4;

            _mockVehicleService
                .Setup(s => s.GetTotalCountAsync(filters))
                .ReturnsAsync(expectedCount);

            var result = await _controller.GetTotalVehicleCount(filters);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualCount = Assert.IsType<int>(okResult.Value);

            Assert.Equal(expectedCount, actualCount);

            _mockVehicleService.Verify(s => s.GetTotalCountAsync(filters), Times.Once);
        }

        [Fact]
        public async Task GetTotalVehicleCount_InvalidFilters_ReturnsBadRequest()
        {
            var filters = new VehicleFilterDto {
                Make = "",
                Model = "",
                StartPrice = -40000,
                EndPrice = -24000,
                Mileage = -10,
                StartYear = 3000,
                EndYear = 1000,
                Gearbox = "",
                Status = ""
            };

            var result = await _controller.GetTotalVehicleCount(filters);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task SearchVehicles_ValidRequest_ReturnsOkWithVehicles()
        {
            var filters = new VehicleFilterDto { Make = "BMW" };
            int pageView = 10;
            int offset = 0;
            string? sortOrder = null;

            var expectedVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, Make = "BMW", Model = "X5" },
                new Vehicle { Id = 2, Make = "BMW", Model = "320i" }
            };

            _mockVehicleService
                .Setup(s => s.SearchVehiclesAsync(filters, pageView, offset, sortOrder))
                .ReturnsAsync(expectedVehicles);

            var result = await _controller.SearchVehicles(filters, pageView, offset, sortOrder);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualVehicles = Assert.IsAssignableFrom<List<Vehicle>>(okResult.Value);

            Assert.Equal(expectedVehicles.Count, actualVehicles.Count);
            Assert.Equal(expectedVehicles, actualVehicles);

            _mockVehicleService.Verify(s => s.SearchVehiclesAsync(filters, pageView, offset, sortOrder), Times.Once);
        }

        [Fact]
        public async Task SearchVehicles_InvalidPagination_ReturnsBadRequest()
        {
            var filters = new VehicleFilterDto { Make = "BMW" };
            int pageView = -1;
            int offset = 0;

            var result = await _controller.SearchVehicles(filters, pageView, offset);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetVehicleById_ReturnsOk_WhenVehicleExists()
        {
            // Arrange
            int vehicleId = 1;
            var expectedVehicle = new Vehicle { Id = vehicleId, Make = "BMW", Model = "X5" };

            _mockVehicleService
                .Setup(s => s.GetVehicleByIdAsync(vehicleId))
                .ReturnsAsync(expectedVehicle);

            // Act
            var result = await _controller.GetVehicleById(vehicleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualVehicle = Assert.IsType<Vehicle>(okResult.Value);

            Assert.Equal(expectedVehicle.Id, actualVehicle.Id);
            Assert.Equal(expectedVehicle.Make, actualVehicle.Make);
            Assert.Equal(expectedVehicle.Model, actualVehicle.Model);
        }

        [Fact]
        public async Task GetVehicleById_ReturnsNotFound_WhenVehicleDoesNotExist()
        {
            // Arrange
            int vehicleId = -1;

            _mockVehicleService
                .Setup(s => s.GetVehicleByIdAsync(vehicleId))
                .ReturnsAsync((Vehicle?)null);

            // Act
            var result = await _controller.GetVehicleById(vehicleId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetVehiclesByModel_ValidModel_ReturnsOk()
        {
            // Arrange
            int pageView = 10;
            int offset = 0;
            string model = "X5";

            var expectedResult = new VehicleListResult
            {
                Vehicles = new List<Vehicle>
                {
                    new Vehicle { Id = 1, Make = "BMW", Model = "X5" }
                },
                TotalCount = 1
            };

            _mockVehicleService
                .Setup(s => s.GetVehiclesByModelAsync(pageView, offset, model))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetVehiclesByModel(pageView, offset, model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualResult = Assert.IsAssignableFrom<VehicleListResult>(okResult.Value);

            Assert.Equal(expectedResult.TotalCount, actualResult.TotalCount);
            Assert.Equal(expectedResult.Vehicles.Count(), actualResult.Vehicles.Count());
        }

        [Fact]
        public async Task GetVehiclesByModel_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            int pageView = 10;
            int offset = 0;
            string invalidModel = ""; 

            // Act
            var result = await _controller.GetVehiclesByModel(pageView, offset, invalidModel);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetVehiclesByModel_InvalidPagination_ReturnsBadRequest()
        {
            // Arrange
            int pageView = -5; 
            int offset = 0;
            string model = "X5";

            // Act
            var result = await _controller.GetVehiclesByModel(pageView, offset, model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
        [Fact]
        public async Task GetVehicleByVin_ReturnsOk_WhenFound()
        {
            // Arrange
            string vin = "5UXKR0C54F0678901";
            var expectedVehicle = new Vehicle { Id = 1, Vin = vin, Make = "BMW", Model = "X5" };

            _mockVehicleService
                .Setup(s => s.GetVehicleByVinAsync(vin))
                .ReturnsAsync(expectedVehicle);

            // Act
            var result = await _controller.GetVehicleByVin(vin);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var vehicle = Assert.IsType<Vehicle>(okResult.Value);

            Assert.Equal(expectedVehicle.Id, vehicle.Id);
        }

        [Fact]
        public async Task GetVehicleByVin_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            string vin = "INVALID_VIN";

            _mockVehicleService
                .Setup(s => s.GetVehicleByVinAsync(vin))
                .ReturnsAsync((Vehicle)null!);

            // Act
            var result = await _controller.GetVehicleByVin(vin);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateVehicle_ReturnsCreated_WhenSuccess()
        {
            // Arrange
            var vehicle = new Vehicle { Id = 1, Vin = "5UXKR0C54F0678901", Make = "BMW", Model = "X5" };

            _mockVehicleService
                .Setup(s => s.GetVehicleByVinAsync(vehicle.Vin))
                .ReturnsAsync((Vehicle)null!);

            _mockVehicleService
                .Setup(s => s.CreateVehicleAsync(vehicle))
                .ReturnsAsync(vehicle);

            // Act
            var result = await _controller.CreateVehicle(vehicle);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdVehicle = Assert.IsType<Vehicle>(createdAtActionResult.Value);

            Assert.Equal(vehicle.Id, createdVehicle.Id);
        }

        [Fact]
        public async Task CreateVehicle_ReturnsBadRequest_WhenDuplicateVin()
        {
            // Arrange
            var vehicle = new Vehicle { Id = 1, Vin = "5UXKR0C54F0678901", Make = "BMW", Model = "X5" };

            _mockVehicleService
                .Setup(s => s.GetVehicleByVinAsync(vehicle.Vin))
                .ReturnsAsync(vehicle);

            // Act
            var result = await _controller.CreateVehicle(vehicle);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateVehicle_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var vehicle = new Vehicle { Id = 1, Vin = "5UXKR0C54F0678901", Make = "BMW", Model = "X5" };

            _mockVehicleService
                .Setup(s => s.GetVehicleByIdAsync(vehicle.Id))
                .ReturnsAsync(vehicle);

            _mockVehicleService
                .Setup(s => s.UpdateVehicleAsync(vehicle))
                .ReturnsAsync(vehicle);

            // Act
            var result = await _controller.UpdateVehicle(vehicle.Id, vehicle);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var updatedVehicle = Assert.IsType<Vehicle>(okResult.Value);

            Assert.Equal(vehicle.Id, updatedVehicle.Id);
        }

        [Fact]
        public async Task UpdateVehicle_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var vehicle = new Vehicle { Id = 1, Vin = "5UXKR0C54F0678901", Make = "BMW", Model = "X5" };

            // Act
            var result = await _controller.UpdateVehicle(99, vehicle);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateVehicle_ReturnsNotFound_WhenVehicleDoesNotExist()
        {
            // Arrange
            var vehicle = new Vehicle { Id = 1, Vin = "5UXKR0C54F0678901", Make = "BMW", Model = "X5" };

            _mockVehicleService
                .Setup(s => s.GetVehicleByIdAsync(vehicle.Id))
                .ReturnsAsync((Vehicle)null!);

            // Act
            var result = await _controller.UpdateVehicle(vehicle.Id, vehicle);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteVehicle_ReturnsNoContent_WhenSuccess()
        {
            // Arrange
            int id = 1;

            _mockVehicleService
                .Setup(s => s.DeleteVehicleAsync(id))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteVehicle(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteVehicle_ReturnsNotFound_WhenNotExist()
        {
            // Arrange
            int id = -1;

            _mockVehicleService
                .Setup(s => s.DeleteVehicleAsync(id))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteVehicle(id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task SaveQuestionnaire_ReturnsOkResultWithLoanAndQuestionnaire()
        {
            // Arrange
            var dto = new QuestionnaireDTO
            {
                DrivingLicense = "Full UK",
                MaritalStatus = "Single",
                DOB = new DateOnly(1995, 1, 1),
                EmploymentStatus = "Employed",
                BorrowAmount = 25000,
                NotSure = false,
                Email = "test@example.com",
                Phone = "1234567890"
            };

            var questionnaire = new Questionnaire
            {
                Id = 1,
                DrivingLicense = dto.DrivingLicense,
                MaritalStatus = dto.MaritalStatus,
                DOB = dto.DOB,
                EmploymentStatus = dto.EmploymentStatus,
                BorrowAmount = dto.BorrowAmount,
                NotSure = dto.NotSure,
                Email = dto.Email,
                Phone = dto.Phone
            };

            var loanRequest = new LoanRequest
            {
                VehicleId = 10,
                LoanAmount = dto.BorrowAmount,
                InterestRate = 7.5m,
                LoanTermMonths = 60
            };

            var loanCalculation = new LoanCalculation
            {
                Id = 1,
                VehicleId = loanRequest.VehicleId,
                LoanAmount = loanRequest.LoanAmount,
                InterestRate = loanRequest.InterestRate,
                LoanTermMonths = loanRequest.LoanTermMonths,
                MonthlyPayment = 500,
                TotalInterest = 5000,
                TotalCost = 30000
            };

            var pdfBytes = new byte[] { 1, 2, 3, 4 };

            _mockVehicleService
                .Setup(s => s.SaveQuestionnaireAsync(dto))
                .ReturnsAsync(questionnaire);

            _mockLoanService
                .Setup(s => s.CalculateLoanAsync(It.Is<LoanRequest>(lr => lr.VehicleId == loanRequest.VehicleId && lr.LoanAmount == loanRequest.LoanAmount)))
                .ReturnsAsync(loanCalculation);

            _mockPdfService
                .Setup(s => s.GenerateLoanPdf(questionnaire, loanCalculation))
                .Returns(pdfBytes);

            _mockEmailService
                .Setup(s => s.SendLoanEmailAsync(dto.Email, pdfBytes))
                .Returns(Task.CompletedTask);

            int vehicleId = 10;

            // Act
            var result = await _controller.SaveQuestionnaire(dto, vehicleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            dynamic response = okResult.Value!;

            // Validate returned Questionnaire
            Assert.Equal(questionnaire.Id, ((Questionnaire)response.Questionnaire).Id);
            Assert.Equal(questionnaire.Email, ((Questionnaire)response.Questionnaire).Email);

            // Validate returned LoanCalculation
            Assert.Equal(loanCalculation.MonthlyPayment, ((LoanCalculation)response.Loan).MonthlyPayment);
            Assert.Equal(loanCalculation.TotalCost, ((LoanCalculation)response.Loan).TotalCost);

            // Verify service calls
            _mockVehicleService.Verify(s => s.SaveQuestionnaireAsync(dto), Times.Once);
            _mockLoanService.Verify(s => s.CalculateLoanAsync(It.IsAny<LoanRequest>()), Times.Once);
            _mockPdfService.Verify(s => s.GenerateLoanPdf(questionnaire, loanCalculation), Times.Once);
            _mockEmailService.Verify(s => s.SendLoanEmailAsync(dto.Email, pdfBytes), Times.Once);
        }

        [Fact]
        public async Task GetAllVehiclesByStatus_ReturnsOk_WithExpectedResult()
        {
            // Arrange
            int pageView = 10, offset = 0;
            string? status = "NEW";

            var expectedResult = new VehicleListResult
            {
                Vehicles = new List<Vehicle>
                {
                    new Vehicle { Id = 1, Make = "BMW", Model = "X5", Status = "NEW" },
                    new Vehicle { Id = 2, Make = "Audi", Model = "Q5", Status = "NEW" }
                },
                TotalCount = 2
            };

            _mockVehicleService
                .Setup(s => s.GetAllVehiclesByStatusAsync(pageView, offset, status))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetAllVehiclesByStatus(pageView, offset, status);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualResult = Assert.IsAssignableFrom<VehicleListResult>(okResult.Value);

            Assert.Equal(expectedResult.TotalCount, actualResult.TotalCount);
            Assert.Equal(expectedResult.Vehicles.Count(), actualResult.Vehicles.Count());

            _mockVehicleService.Verify(s => s.GetAllVehiclesByStatusAsync(pageView, offset, status), Times.Once);
        }

        [Fact]
        public async Task GetAllVehiclesByStatus_InvalidPagination_ReturnsBadRequest()
        {
            // Arrange
            int pageView = -5, offset = 0;
            string? status = "NEW";

            // Act
            var result = await _controller.GetAllVehiclesByStatus(pageView, offset, status);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCarFeatures_ReturnsOk_WhenFeatureFound()
        {
            // Arrange
            var make = "BMW";
            var model = "X5";

            var carFeatures = new List<VehicleModelJSON>
            {
                new VehicleModelJSON { Make = "BMW", Model = "X5", Year = 2022 },
                new VehicleModelJSON { Make = "Audi", Model = "A4", Year = 2022  }
            };

            var expectedFeature = carFeatures.First(c => c.Make == make && c.Model == model);

            _mockVehicleService
                .Setup(s => s.GetAllCarFeaturesAsync())
                .ReturnsAsync(carFeatures);

            _mockVehicleService
                .Setup(s => s.GetCarFeature(carFeatures, make, model))
                .Returns(expectedFeature);

            // Act
            var result = await _controller.GetCarFeatures(make, model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var actualFeature = Assert.IsType<NormalizedCarFeatureDto>(okResult.Value);

            Assert.Equal(expectedFeature.Make, actualFeature.Make);
            Assert.Equal(expectedFeature.Model, actualFeature.Model);
            Assert.Equal(expectedFeature.Features?.Engine?.Type, actualFeature.Features?.Engine?.Type);
            Assert.Equal(expectedFeature.Features?.Engine?.Horsepower, actualFeature?.Features?.Engine?.Horsepower);
            

            _mockVehicleService.Verify(s => s.GetAllCarFeaturesAsync(), Times.Once);
            _mockVehicleService.Verify(s => s.GetCarFeature(carFeatures, make, model), Times.Once);
        }

        [Fact]
        public async Task GetCarFeatures_ReturnsBadRequest_WhenInvalidMakeOrModel()
        {
            // Arrange
            var make = ""; 
            var model = "X5";

            // Act
            var result = await _controller.GetCarFeatures(make, model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCarFeatures_ReturnsNotFound_WhenNoFeaturesFound()
        {
            // Arrange
            var make = "BMW";
            var model = "X5";

            _mockVehicleService
                .Setup(s => s.GetAllCarFeaturesAsync())
                .ReturnsAsync(new List<VehicleModelJSON>());

            // Act
            var result = await _controller.GetCarFeatures(make, model);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCarFeatures_ReturnsNotFound_WhenNoMatchFound()
        {
            // Arrange
            var make = "BMW";
            var model = "X6";

            var carFeatures = new List<VehicleModelJSON>
            {
                new VehicleModelJSON { Make = "BMW", Model = "X5", Year = 2022 }
            };

            _mockVehicleService
                .Setup(s => s.GetAllCarFeaturesAsync())
                .ReturnsAsync(carFeatures);

            _mockVehicleService
                .Setup(s => s.GetCarFeature(carFeatures, make, model))
                .Returns((VehicleModelJSON?)null);

            // Act
            var result = await _controller.GetCarFeatures(make, model);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);

            _mockVehicleService.Verify(s => s.GetAllCarFeaturesAsync(), Times.Once);
            _mockVehicleService.Verify(s => s.GetCarFeature(carFeatures, make, model), Times.Once);
        }

    }
}
