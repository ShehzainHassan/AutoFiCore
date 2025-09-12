using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly Mock<ITokenProvider> _tokenProviderMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<IUserRepository> _unitOfWorkUsersMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _tokenProviderMock = new Mock<ITokenProvider>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
            _unitOfWorkUsersMock = new Mock<IUserRepository>();
            _unitOfWorkMock.SetupGet(u => u.Users).Returns(_unitOfWorkUsersMock.Object);
            _service = new UserService(
                _userRepositoryMock.Object,
                _loggerMock.Object,
                _tokenProviderMock.Object,
                _unitOfWorkMock.Object,
                _refreshTokenServiceMock.Object
            );
        }

        [Fact]
        public async Task AddUserAsync_ReturnsFailure_WhenEmailExists()
        {
            var user = new User { Email = "test@test.com" };
            _userRepositoryMock.Setup(r => r.IsEmailExists(user.Email)).ReturnsAsync(true);

            var result = await _service.AddUserAsync(user);

            Assert.False(result.IsSuccess);
            Assert.Equal("User already exists", result.Error);
        }

        [Fact]
        public async Task AddUserAsync_ReturnsSuccess_WhenUserCreated()
        {
            var user = new User { Email = "test@test.com" };
            _userRepositoryMock.Setup(r => r.IsEmailExists(user.Email)).ReturnsAsync(false);
            _unitOfWorkUsersMock.Setup(r => r.AddUserAsync(user)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.AddUserAsync(user);

            Assert.True(result.IsSuccess);
            Assert.Equal(user, result.Value);
        }

        [Fact]
        public async Task LoginUserAsync_DelegatesToRepository()
        {
            var email = "test@test.com";
            var password = "pass";
            var authResponse = new AuthResponse { UserId = 1, UserName = "Test", UserEmail = email };
            _userRepositoryMock.Setup(r => r.LoginUserAsync(email, password, _tokenProviderMock.Object, _refreshTokenServiceMock.Object))
                .ReturnsAsync(authResponse);

            var result = await _service.LoginUserAsync(email, password);

            Assert.Equal(authResponse, result);
        }

        [Fact]
        public async Task AddUserLikeAsync_AddsLikeAndSaves()
        {
            var like = new UserLikes { userId = 1 };
            _unitOfWorkUsersMock.Setup(r => r.AddUserLikeAsync(like)).ReturnsAsync(like);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.AddUserLikeAsync(like);

            Assert.Equal(like, result);
            _unitOfWorkUsersMock.Verify(r => r.AddUserLikeAsync(like), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveUserLikeAsync_RemovesLikeAndSaves()
        {
            var like = new UserLikes { userId = 1 };
            _unitOfWorkUsersMock.Setup(r => r.RemoveUserLikeAsync(like)).ReturnsAsync(like);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.RemoveUserLikeAsync(like);

            Assert.Equal(like, result);
            _unitOfWorkUsersMock.Verify(r => r.RemoveUserLikeAsync(like), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveSavedSearchAsync_RemovesSearchAndSaves()
        {
            var search = new UserSavedSearch { userId = 1 };
            _unitOfWorkUsersMock.Setup(r => r.RemoveUserSearchAsync(search)).ReturnsAsync(search);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.RemoveSavedSearchAsync(search);

            Assert.Equal(search, result);
            _unitOfWorkUsersMock.Verify(r => r.RemoveUserSearchAsync(search), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser()
        {
            var user = new User { Id = 1 };
            _userRepositoryMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

            var result = await _service.GetUserByIdAsync(1);

            Assert.Equal(user, result);
        }

        [Fact]
        public async Task AddUserSearchAsync_AddsSearchAndSaves()
        {
            var search = new UserSavedSearch { userId = 1 };
            _unitOfWorkUsersMock.Setup(r => r.AddUserSearchAsync(search)).ReturnsAsync(search);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.AddUserSearchAsync(search);

            Assert.Equal(search, result);
            _unitOfWorkUsersMock.Verify(r => r.AddUserSearchAsync(search), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUserLikedVinsAsync_ReturnsVins()
        {
            var vins = new List<string> { "VIN1", "VIN2" };
            _userRepositoryMock.Setup(r => r.GetUserLikesVehicles(1)).ReturnsAsync(vins);

            var result = await _service.GetUserLikedVinsAsync(1);

            Assert.Equal(vins, result);
        }

        [Fact]
        public async Task GetUserSavedSearches_ReturnsSearches()
        {
            var searches = new List<string> { "search1", "search2" };
            _userRepositoryMock.Setup(r => r.GetUserSavedSearches(1)).ReturnsAsync(searches);

            var result = await _service.GetUserSavedSearches(1);

            Assert.Equal(searches, result);
        }

        [Fact]
        public async Task AddUserInteractionAsync_AddsInteractionAndSaves()
        {
            var interaction = new UserInteractions { UserId = 1 };
            _unitOfWorkUsersMock.Setup(r => r.AddUserInteraction(interaction)).ReturnsAsync(interaction);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.AddUserInteractionAsync(interaction);

            Assert.Equal(interaction, result);
            _unitOfWorkUsersMock.Verify(r => r.AddUserInteraction(interaction), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersCountAsync_ReturnsCount()
        {
            _unitOfWorkUsersMock.Setup(r => r.GetAllUsersCountAsync()).ReturnsAsync(5);

            var result = await _service.GetAllUsersCountAsync();

            Assert.Equal(5, result);
        }

        [Fact]
        public async Task GetOldestUserCreatedDateAsync_ReturnsDate()
        {
            var date = DateTime.UtcNow;
            _unitOfWorkUsersMock.Setup(r => r.GetOldestUserCreatedDateAsync()).ReturnsAsync(date);

            var result = await _service.GetOldestUserCreatedDateAsync();

            Assert.Equal(date, result);
        }
    }
}