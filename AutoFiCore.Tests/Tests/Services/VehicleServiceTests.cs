using Xunit;
using Moq;
using AutoFiCore.Services;
using AutoFiCore.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using AutoFiCore.Dto;
using AutoFiCore.Models;

namespace AutoFiCore.Tests.Services
{
    public class VehicleServiceTests
    {
        private readonly Mock<IVehicleRepository> _mockRepository;
        private readonly Mock<ILogger<VehicleService>> _mockLogger;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly VehicleService _vehicleService;

        public VehicleServiceTests()
        {
            _mockRepository = new Mock<IVehicleRepository>();
            _mockLogger = new Mock<ILogger<VehicleService>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cachedVehicleRepository = new CachedVehicleRepository(_mockRepository.Object, memoryCache);

            _vehicleService = new VehicleService(
                _mockRepository.Object,
                _mockLogger.Object,
                cachedVehicleRepository,
                _mockUnitOfWork.Object
            );
        }

        [Fact]
        public async Task GetAllVehiclesMakesAsync_Returns_Makes_List()
        {
            var expectedMakes = new List<string> { "BMW", "Mercedes", "Audi" };

            _mockRepository.Setup(r => r.GetAllVehicleMakes()).ReturnsAsync(expectedMakes);

            var result = await _vehicleService.GetAllVehiclesMakesAsync();

            Assert.NotNull(result);
            Assert.Equal(expectedMakes, result);

            _mockRepository.Verify(r => r.GetAllVehicleMakes(), Times.Once);
        }

        [Fact]
        public async Task GetVehiclesByMakeAsync_ReturnsExpectedResult()
        {
            // Arrange
            int pageView = 10;
            int offset = 0;
            string make = "BMW";

            var expectedVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, Make = "BMW", Model = "X5" },
                new Vehicle { Id = 2, Make = "BMW", Model = "X5" },
                new Vehicle { Id = 3, Make = "BMW", Model = "X5" },
                new Vehicle { Id = 4, Make = "Audi", Model = "X5" },

            };

            var expectedResult = new VehicleListResult
            {
                Vehicles = expectedVehicles,
                TotalCount = expectedVehicles.Count
            };

            _mockRepository
                .Setup(r => r.GetVehiclesByMakeAsync(pageView, offset, make))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _vehicleService.GetVehiclesByMakeAsync(pageView, offset, make);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.TotalCount, result.TotalCount);
            Assert.Equal(expectedResult.Vehicles.Count(), result.Vehicles.Count());
            Assert.Equal(expectedResult.Vehicles, result.Vehicles);

            _mockRepository.Verify(r => r.GetVehiclesByMakeAsync(pageView, offset, make), Times.Once);
        }

        [Fact]
        public async Task GetDistinctColorsAsync_Returns_Color_List()
        {
            var expectedColors = new List<string> { "Red", "Blue", "Green" };

            _mockRepository.Setup(r => r.GetDistinctColorsAsync()).ReturnsAsync(expectedColors);

            var result = await _vehicleService.GetDistinctColorsAsync();

            Assert.NotNull(result);
            Assert.Equal(expectedColors, result);

            _mockRepository.Verify(r => r.GetDistinctColorsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAvailableColorsCountAsync_ReturnsExpectedCounts()
        {
            var filters = new VehicleFilterDto { Make = "BMW" };
            var expectedCounts = new Dictionary<string, int> { { "Red", 3 }, { "Blue", 2 } };

            _mockRepository.Setup(r => r.GetColorsCountsAsync(filters)).ReturnsAsync(expectedCounts);

            var result = await _vehicleService.GetAvailableColorsCountAsync(filters);

            Assert.Equal(expectedCounts, result);

            _mockRepository.Verify(r => r.GetColorsCountsAsync(filters), Times.Once);
        }

        [Fact]
        public async Task GetGearboxCountsAsync_ReturnsCorrectCounts()
        {
            var filters = new VehicleFilterDto { Make = "BMW" };
            var expectedCounts = new Dictionary<string, int> { { "Automatic", 10 }, { "Manual", 5 } };

            _mockRepository.Setup(r => r.GetGearboxCountsAsync(filters)).ReturnsAsync(expectedCounts);

            var result = await _vehicleService.GetGearboxCountsAsync(filters);

            Assert.Equal(expectedCounts, result);

            _mockRepository.Verify(r => r.GetGearboxCountsAsync(filters), Times.Once);
        }

        [Fact]
        public async Task GetTotalCountAsync_ReturnsExpectedCount()
        {
            var filters = new VehicleFilterDto { Make = "BMW" };
            int expectedCount = 5;

            _mockRepository.Setup(r => r.GetTotalCountAsync(filters)).ReturnsAsync(expectedCount);

            var result = await _vehicleService.GetTotalCountAsync(filters);

            Assert.Equal(expectedCount, result);

            _mockRepository.Verify(r => r.GetTotalCountAsync(filters), Times.Once);
        }

        [Fact]
        public async Task SearchVehiclesAsync_ReturnsExpectedVehicles()
        {
            var filters = new VehicleFilterDto { Make = "BMW" };
            int pageView = 10;
            int offset = 0;
            string? sortOrder = null;

            var expectedVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, Make = "BMW", Model = "X5" },
            };

            _mockRepository
                .Setup(r => r.SearchVehiclesAsync(filters, pageView, offset, sortOrder))
                .ReturnsAsync(expectedVehicles);

            var result = await _vehicleService.SearchVehiclesAsync(filters, pageView, offset, sortOrder);

            Assert.NotNull(result);
            Assert.Equal(expectedVehicles, result);

            _mockRepository.Verify(r => r.SearchVehiclesAsync(filters, pageView, offset, sortOrder), Times.Once);
        }
       
        [Fact]
        public async Task GetVehicleByIdAsync_ReturnsVehicle_WhenExists()
        {
            // Arrange
            int vehicleId = 1;
            var expectedVehicle = new Vehicle { Id = vehicleId, Make = "BMW", Model = "X5" };

            _mockRepository
                .Setup(r => r.GetVehicleByIdAsync(vehicleId))
                .ReturnsAsync(expectedVehicle);

            // Act
            var result = await _vehicleService.GetVehicleByIdAsync(vehicleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedVehicle.Id, result.Id);
            Assert.Equal(expectedVehicle.Make, result.Make);
            Assert.Equal(expectedVehicle.Model, result.Model);

            _mockRepository.Verify(r => r.GetVehicleByIdAsync(vehicleId), Times.Once);
        }

        [Fact]
        public async Task GetVehiclesByModelAsync_ReturnsExpectedResult()
        {
            // Arrange
            int pageView = 10;
            int offset = 0;
            string model = "X5";

            var expectedResult = new VehicleListResult
            {
                Vehicles = new List<Vehicle>
                {
                    new Vehicle { Id = 1, Make = "BMW", Model = "X5" },
                },
                TotalCount = 2
            };

            _mockRepository
                .Setup(r => r.GetVehiclesByModelAsync(pageView, offset, model))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _vehicleService.GetVehiclesByModelAsync(pageView, offset, model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.TotalCount, result.TotalCount);
            Assert.Equal(expectedResult.Vehicles.Count(), result.Vehicles.Count());

            _mockRepository.Verify(r => r.GetVehiclesByModelAsync(pageView, offset, model), Times.Once);
        }

        [Fact]
        public async Task GetVehicleByVinAsync_ReturnsExpectedVehicle()
        {
            // Arrange
            string vin = "5UXKR0C54F0678901";
            var expectedVehicle = new Vehicle { Id = 1, Vin = vin, Make = "BMW", Model = "X5" };

            _mockRepository
                .Setup(r => r.GetVehicleByVinAsync(vin))
                .ReturnsAsync(expectedVehicle);

            // Act
            var result = await _vehicleService.GetVehicleByVinAsync(vin);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedVehicle.Id, result.Id);
            Assert.Equal(expectedVehicle.Vin, result.Vin);

            _mockRepository.Verify(r => r.GetVehicleByVinAsync(vin), Times.Once);
        }

        [Fact]
        public async Task CreateVehicleAsync_ReturnsCreatedVehicle()
        {
            // Arrange
            var vehicle = new Vehicle { Id = 1, Vin = "5UXKR0C54F0678901", Make = "BMW", Model = "X5" };

            _mockUnitOfWork
                .Setup(u => u.Vehicles.AddVehicleAsync(vehicle))
                .ReturnsAsync(vehicle);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _vehicleService.CreateVehicleAsync(vehicle);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(vehicle.Id, result.Id);
            Assert.Equal(vehicle.Vin, result.Vin);

            _mockUnitOfWork.Verify(u => u.Vehicles.AddVehicleAsync(vehicle), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateVehicleAsync_Success_ReturnsUpdatedVehicle()
        {
            // Arrange
            var vehicle = new Vehicle { Id = 1, Vin = "5UXKR0C54F0678901", Make = "BMW", Model = "X5" };

            _mockUnitOfWork
                .Setup(u => u.Vehicles.UpdateVehicleAsync(vehicle))
                .ReturnsAsync(true);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _vehicleService.UpdateVehicleAsync(vehicle);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(vehicle.Id, result.Id);

            _mockUnitOfWork.Verify(u => u.Vehicles.UpdateVehicleAsync(vehicle), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateVehicleAsync_Failure_ThrowsInvalidOperationException()
        {
            // Arrange
            var vehicle = new Vehicle { Id = 1, Vin = "5UXKR0C54F0678901", Make = "BMW", Model = "X5" };

            _mockUnitOfWork
                .Setup(u => u.Vehicles.UpdateVehicleAsync(vehicle))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _vehicleService.UpdateVehicleAsync(vehicle));

            _mockUnitOfWork.Verify(u => u.Vehicles.UpdateVehicleAsync(vehicle), Times.Once);
        }

        [Fact]
        public async Task DeleteVehicleAsync_ReturnsTrue()
        {
            // Arrange
            int id = 1;

            _mockUnitOfWork
                .Setup(u => u.Vehicles.DeleteVehicleAsync(id))
                .ReturnsAsync(true);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _vehicleService.DeleteVehicleAsync(id);

            // Assert
            Assert.True(result);

            _mockUnitOfWork.Verify(u => u.Vehicles.DeleteVehicleAsync(id), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteVehicleAsync_ReturnsFalse_WhenVehicleNotFound()
        {
            // Arrange
            int id = 0;

            _mockUnitOfWork
                .Setup(u => u.Vehicles.DeleteVehicleAsync(id))
                .ReturnsAsync(false);

            // Act
            var result = await _vehicleService.DeleteVehicleAsync(id);

            // Assert
            Assert.False(result);

            _mockUnitOfWork.Verify(u => u.Vehicles.DeleteVehicleAsync(id), Times.Once);
        }
        [Fact]
        public async Task SaveQuestionnaireAsync_ReturnsSavedQuestionnaire()
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

            var expectedQuestionnaire = new Questionnaire
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

            _mockRepository
                .Setup(r => r.SaveQuestionnaireAsync(dto))
                .ReturnsAsync(expectedQuestionnaire);

            // Act
            var result = await _vehicleService.SaveQuestionnaireAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedQuestionnaire.Id, result.Id);
            Assert.Equal(expectedQuestionnaire.Email, result.Email);

            _mockRepository.Verify(r => r.SaveQuestionnaireAsync(dto), Times.Once);
        }
     
        [Fact]
        public async Task GetAllVehiclesByStatusAsync_ReturnsExpectedResult()
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

            _mockRepository
                .Setup(r => r.GetAllVehiclesByStatusAsync(pageView, offset, status))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _vehicleService.GetAllVehiclesByStatusAsync(pageView, offset, status);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.TotalCount, result.TotalCount);
            Assert.Equal(expectedResult.Vehicles.Count(), result.Vehicles.Count());

            _mockRepository.Verify(r => r.GetAllVehiclesByStatusAsync(pageView, offset, status), Times.Once);
        }

        [Fact]
        public void GetCarFeature_ReturnsExpectedFeature()
        {
            // Arrange
            var carFeatures = new List<VehicleModelJSON>
            {
                new VehicleModelJSON { Make = "BMW", Model = "X5", Year = 2022 },
                new VehicleModelJSON { Make = "Audi", Model = "A4", Year = 2022 }
            };

            string make = "BMW";
            string model = "X5";

            _mockRepository
                .Setup(r => r.GetCarFeature(carFeatures, make, model))
                .Returns(carFeatures.FirstOrDefault(c => c.Make == make && c.Model == model));

            // Act
            var result = _vehicleService.GetCarFeature(carFeatures, make, model);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("BMW", result.Make);
            Assert.Equal("X5", result.Model);

            _mockRepository.Verify(r => r.GetCarFeature(carFeatures, make, model), Times.Once);
        }

        [Fact]
        public async Task GetAllCarFeaturesAsync_ReturnsExpectedList()
        {
            // Arrange
            var expectedFeatures = new List<VehicleModelJSON>
            {
                new VehicleModelJSON { Make = "BMW", Model = "X5", Year = 2022 },
                new VehicleModelJSON { Make = "Audi", Model = "A4", Year = 2022}
            };

            _mockRepository
                .Setup(r => r.GetAllCarFeaturesAsync())
                .ReturnsAsync(expectedFeatures);

            // Act
            var result = await _vehicleService.GetAllCarFeaturesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedFeatures.Count, result.Count);
            Assert.Equal(expectedFeatures.First().Make, result.First().Make);

            _mockRepository.Verify(r => r.GetAllCarFeaturesAsync(), Times.Once);
        }

    }
}
