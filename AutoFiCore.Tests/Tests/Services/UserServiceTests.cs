using AutoFiCore.Data;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AutoFiCore.Dto;
using Microsoft.Extensions.Configuration;
namespace AutoFiCore.Tests.Tests.Controllers
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly TokenProvider _tokenProvider;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _mockRepository = new Mock<IUserRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _tokenProvider = new TokenProvider(_mockConfiguration.Object);

            _mockUnitOfWork.Setup(u => u.Users).Returns(_mockRepository.Object);

            _service = new UserService(_mockRepository.Object, _mockLogger.Object, _tokenProvider, _mockUnitOfWork.Object);
        }

        [Fact]
        public async Task AddUserAsync_ShouldReturnSuccess_WhenUserIsAdded()
        {
            var user = new User
            {
                Id = 26,
                Name = "John",
                Email = "john@example.com",
                Password = "1234ABCa@"
            };

            _mockRepository.Setup(r => r.IsEmailExists(user.Email))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) =>
                {
                    u.Password = BCrypt.Net.BCrypt.HashPassword(u.Password);
                    return u;
                });

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.AddUserAsync(user);

            if (!result.IsSuccess)
            {
                Console.WriteLine("Errors: " + string.Join(", ", result.Errors));
                Console.WriteLine("ErrorMessage: " + result.Error);
            }

            Assert.NotNull(result.Value);
            Assert.True(result.IsSuccess);
            Assert.Equal(user.Id, result.Value.Id);

            _mockRepository.Verify(r => r.AddUserAsync(It.Is<User>(u => u.Email == user.Email)), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task LoginUserAsync_ShouldReturnAuthResponse_WhenCredentialsAreValid()
        {
            var user = new User
            {
                Id = 2,
                Name = "John",
                Email = "john@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("MyPassword")
            };

            _mockRepository.Setup(r => r.LoginUserAsync(user.Email, "MyPassword", It.IsAny<TokenProvider>()))
                .ReturnsAsync(new AuthResponse
                {
                    Token = "mock-jwt-token",
                    UserId = user.Id,
                    UserName = user.Name,
                    UserEmail = user.Email
                });

            var result = await _service.LoginUserAsync(user.Email, "MyPassword");

            Assert.NotNull(result);
            Assert.Equal(user.Email, result!.UserEmail);
            Assert.Equal(user.Id, result.UserId);

            _mockRepository.Verify(r => r.LoginUserAsync(user.Email, "MyPassword", It.IsAny<TokenProvider>()), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            int userId = 5;

            var user = new User
            {
                Id = userId,
                Name = "Bob",
                Email = "bob@example.com"
            };

            _mockRepository.Setup(r => r.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            var result = await _service.GetUserByIdAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(userId, result!.Id);
            Assert.Equal("Bob", result.Name);

            _mockRepository.Verify(r => r.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task AddUserLikeAsync_ShouldReturnLike_WhenAdded()
        {
            var like = new UserLikes { userId = 1, vehicleVin = "1FMCU9J97FUA88429" };

            _mockRepository.Setup(r => r.AddUserLikeAsync(like))
                .ReturnsAsync(like);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.AddUserLikeAsync(like);

            Assert.Equal(like, result);
            _mockRepository.Verify(r => r.AddUserLikeAsync(like), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveUserLikeAsync_ShouldReturnLike_WhenRemoved()
        {
            var like = new UserLikes { userId = 1, vehicleVin = "1FMCU9J97FUA88429" };

            _mockRepository.Setup(r => r.RemoveUserLikeAsync(like))
                .ReturnsAsync(like);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.RemoveUserLikeAsync(like);

            Assert.Equal(like, result);
            _mockRepository.Verify(r => r.RemoveUserLikeAsync(like), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddUserSearchAsync_ShouldReturnSearch_WhenAdded()
        {
            var search = new UserSavedSearch { userId = 1, search = "BMW" };

            _mockRepository.Setup(r => r.AddUserSearchAsync(search))
                .ReturnsAsync(search);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.AddUserSearchAsync(search);

            Assert.Equal(search, result);
            _mockRepository.Verify(r => r.AddUserSearchAsync(search), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveSavedSearchAsync_ShouldReturnSearch_WhenRemoved()
        {
            var search = new UserSavedSearch { userId = 1, search = "BMW" };

            _mockRepository.Setup(r => r.RemoveUserSearchAsync(search))
                .ReturnsAsync(search);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.RemoveSavedSearchAsync(search);

            Assert.Equal(search, result);
            _mockRepository.Verify(r => r.RemoveUserSearchAsync(search), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUserLikedVinsAsync_ShouldReturnVinList()
        {
            int userId = 2;

            var vins = new List<string> { "1FMCU9J97FUA88429", "1FMCU9J97FUA88429" };

            _mockRepository.Setup(r => r.GetUserLikesVehicles(userId))
                .ReturnsAsync(vins);

            var result = await _service.GetUserLikedVinsAsync(userId);

            Assert.Equal(vins, result);
            _mockRepository.Verify(r => r.GetUserLikesVehicles(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserSavedSearches_ShouldReturnSearchList()
        {
            int userId = 3;

            var searches = new List<string> { "BMW", "Audi" };

            _mockRepository.Setup(r => r.GetUserSavedSearches(userId))
                .ReturnsAsync(searches);

            var result = await _service.GetUserSavedSearches(userId);

            Assert.Equal(searches, result);
            _mockRepository.Verify(r => r.GetUserSavedSearches(userId), Times.Once);
        }

        [Fact]
        public async Task AddUserInteractionAsync_ShouldReturnInteraction_WhenAdded()
        {
            var interaction = new UserInteractions
            {
                UserId = 1,
                VehicleId = 1,
                InteractionType = "view"
            };

            _mockRepository.Setup(r => r.AddUserInteraction(interaction))
                .ReturnsAsync(interaction);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.AddUserInteractionAsync(interaction);

            Assert.Equal(interaction, result);
            _mockRepository.Verify(r => r.AddUserInteraction(interaction), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
