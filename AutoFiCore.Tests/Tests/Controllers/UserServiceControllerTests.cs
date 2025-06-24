using AutoFiCore.Controllers;
using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFiCore.Utilities;

namespace AutoFiCore.Tests.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IVehicleService> _mockVehicleService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockVehicleService = new Mock<IVehicleService>();
            _controller = new UserController(_mockUserService.Object, _mockVehicleService.Object);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnOk_WhenUserCreated()
        {
            var user = new User { Id = 1, Name = "John", Email = "john@example.com", Password = "1234ABCa@" };
            var result = Result<User>.Success(user);

            _mockUserService.Setup(s => s.AddUserAsync(user)).ReturnsAsync(result);

            var response = await _controller.CreateUser(user);

            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(user, okResult.Value);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnOk_WhenLoginSuccessful()
        {
            var loginDto = new LoginDTO { Email = "test@example.com", Password = "1234ABCa@" };
            var authResponse = new AuthResponse { UserId = 1, UserName = "Test", UserEmail = loginDto.Email, Token = "abc" };

            _mockUserService.Setup(s => s.LoginUserAsync(loginDto.Email, loginDto.Password)).ReturnsAsync(authResponse);

            var result = await _controller.LoginUser(loginDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result); 

            Assert.Equal(authResponse, okResult.Value);
        }

        [Fact]
        public async Task AddUserLike_ShouldReturnOk_WhenLikeAdded()
        {
            var like = new UserLikes { userId = 1, vehicleVin = "1FTEW1EP7JFA54321" };

            _mockUserService.Setup(s => s.GetUserByIdAsync(like.userId)).ReturnsAsync(new User());
            _mockVehicleService.Setup(v => v.GetVehicleByVinAsync(like.vehicleVin)).ReturnsAsync(new Vehicle());
            _mockUserService.Setup(s => s.AddUserLikeAsync(like)).ReturnsAsync(like);

            var result = await _controller.AddUserLike(like);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var returnedLike = Assert.IsType<UserLikes>(okResult.Value);
            Assert.Equal(like.userId, returnedLike.userId);
            Assert.Equal(like.vehicleVin, returnedLike.vehicleVin);
        }

        [Fact]
        public async Task RemoveUserLike_ShouldReturnOk_WhenLikeRemoved()
        {
            var like = new UserLikes { userId = 1, vehicleVin = "1FTEW1EP7JFA54321" };

            _mockUserService.Setup(s => s.GetUserByIdAsync(like.userId)).ReturnsAsync(new User());
            _mockVehicleService.Setup(v => v.GetVehicleByVinAsync(like.vehicleVin)).ReturnsAsync(new Vehicle());
            _mockUserService.Setup(s => s.RemoveUserLikeAsync(like)).ReturnsAsync(like);

            var result = await _controller.RemoveUserLike(like);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            Assert.Equal(like, okResult.Value);
        }

        [Fact]
        public async Task GetUserLikedVins_ShouldReturnOk_WhenUserExists()
        {
            int userId = 1;
            var vins = new List<string> { "1FTEW1EP7JFA54321", "1FMCU9J97FUA88429" };

            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(new User());
            _mockUserService.Setup(s => s.GetUserLikedVinsAsync(userId)).ReturnsAsync(vins);

            var result = await _controller.GetUserLikedVins(userId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(vins, okResult.Value);
        }

        [Fact]
        public async Task GetUserSavedSearches_ShouldReturnOk_WhenUserExists()
        {
            int userId = 1;
            var searches = new List<string> { "Audi", "BMW" };

            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(new User());
            _mockUserService.Setup(s => s.GetUserSavedSearches(userId)).ReturnsAsync(searches);

            var result = await _controller.GetUserSearches(userId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(searches, okResult.Value);
        }

        [Fact]
        public async Task DeleteUserSearch_ShouldReturnOk_WhenSearchRemoved()
        {
            var search = new UserSavedSearch { userId = 1, search = "BMW" };

            _mockUserService.Setup(s => s.GetUserByIdAsync(search.userId)).ReturnsAsync(new User());
            _mockUserService.Setup(s => s.RemoveSavedSearchAsync(search)).ReturnsAsync(search);

            var result = await _controller.DeleteUserSearch(search);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var returnedSearch = Assert.IsType<UserSavedSearch>(okResult.Value);
            Assert.Equal(search.userId, returnedSearch.userId);
            Assert.Equal(search.search, returnedSearch.search);
        }

        [Fact]
        public async Task SaveUserSearch_ShouldReturnOk_WhenSearchSaved()
        {
            var search = new UserSavedSearch { userId = 1, search = "BMW" };

            _mockUserService.Setup(s => s.GetUserByIdAsync(search.userId)).ReturnsAsync(new User());
            _mockUserService.Setup(s => s.AddUserSearchAsync(search)).ReturnsAsync(search);

            var result = await _controller.SaveUserSearch(search);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var returnedSearch = Assert.IsType<UserSavedSearch>(okResult.Value);
            Assert.Equal(search.userId, returnedSearch.userId);
            Assert.Equal(search.search, returnedSearch.search);
        }

        [Fact]
        public async Task AddUserInteraction_ShouldReturnOk_WhenInteractionSaved()
        {
            var interaction = new UserInteractions
            {
                Id = 1,
                UserId = 1,
                VehicleId = 10,
                InteractionType = "view",
                CreatedAt = DateTime.UtcNow
            };

            _mockUserService.Setup(s => s.GetUserByIdAsync(interaction.UserId)).ReturnsAsync(new User());
            _mockVehicleService.Setup(v => v.GetVehicleByIdAsync(interaction.VehicleId)).ReturnsAsync(new Vehicle());
            _mockUserService.Setup(s => s.AddUserInteractionAsync(interaction)).ReturnsAsync(interaction);

            var result = await _controller.AddUserInteraction(interaction);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            var returnedDto = Assert.IsType<UserInteractionsDTO>(okResult.Value);
            Assert.Equal(interaction.Id, returnedDto.Id);
            Assert.Equal(interaction.UserId, returnedDto.UserId);
            Assert.Equal(interaction.VehicleId, returnedDto.VehicleId);
            Assert.Equal(interaction.InteractionType, returnedDto.InteractionType);
        }

    }
}
