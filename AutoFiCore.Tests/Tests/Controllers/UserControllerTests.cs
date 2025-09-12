using AutoFiCore.Controllers;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IVehicleService> _vehicleServiceMock;
        private readonly Mock<ITokenProvider> _tokenProviderMock;
        private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
        private readonly Mock<ILogger<UserController>> _loggerMock;
        private readonly TestUserController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _vehicleServiceMock = new Mock<IVehicleService>();
            _tokenProviderMock = new Mock<ITokenProvider>();
            _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
            _loggerMock = new Mock<ILogger<UserController>>();
            _controller = new TestUserController(
                _userServiceMock.Object,
                _vehicleServiceMock.Object,
                _tokenProviderMock.Object,
                _refreshTokenServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateUser_ReturnsOk_WhenSuccess()
        {
            var user = new User();
            var result = Result<User>.Success(user);
            _userServiceMock.Setup(s => s.AddUserAsync(user)).ReturnsAsync(result);

            var response = await _controller.CreateUser(user);

            var ok = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(user, ok.Value);
        }

        [Fact]
        public async Task CreateUser_ReturnsBadRequest_WhenErrors()
        {
            var user = new User();
            var result = Result<User>.Failure(new List<string> { "error1" });
            _userServiceMock.Setup(s => s.AddUserAsync(user)).ReturnsAsync(result);

            var response = await _controller.CreateUser(user);

            var bad = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Contains("errors", bad.Value.ToString());
        }


        [Fact]
        public async Task CreateUser_ReturnsConflict_WhenError()
        {
            var user = new User();
            var result = Result<User>.Failure("User already exists");

            _userServiceMock.Setup(s => s.AddUserAsync(user)).ReturnsAsync(result);

            var response = await _controller.CreateUser(user);

            var conflictResult = Assert.IsType<ConflictObjectResult>(response);
            var payload = conflictResult.Value as IDictionary<string, object>;
            Assert.NotNull(payload);
            Assert.Contains("User already exists", payload["message"].ToString());
        }



        [Fact]
        public async Task LoginUser_ReturnsOk_WhenSuccess()
        {
            var login = new LoginDTO { Email = "a", Password = "b" };
            var auth = new AuthResponse { UserId = 1, UserName = "n", UserEmail = "e" };
            var refreshToken = new RefreshToken { Token = "rt", Expires = DateTime.UtcNow.AddDays(1) };
            
            _userServiceMock.Setup(s => s.LoginUserAsync(login.Email, login.Password)).ReturnsAsync(auth);
            _refreshTokenServiceMock.Setup(s => s.GetLatestTokenForUserAsync(auth.UserId)).ReturnsAsync(refreshToken);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext = httpContext;

            var response = await _controller.LoginUser(login);

            var ok = Assert.IsType<OkObjectResult>(response.Result);
            Assert.Equal(auth, ok.Value);

            Assert.True(httpContext.Response.Headers.ContainsKey("Set-Cookie"));
            Assert.Contains("refreshToken=rt", httpContext.Response.Headers["Set-Cookie"].ToString());
        }

        [Fact]
        public async Task LoginUser_ReturnsUnauthorized_WhenNull()
        {
            var login = new LoginDTO { Email = "a", Password = "b" };
            _userServiceMock.Setup(s => s.LoginUserAsync(login.Email, login.Password)).ReturnsAsync((AuthResponse)null!);

            var response = await _controller.LoginUser(login);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(response.Result);
            Assert.Contains("Invalid credentials", unauthorized.Value!.ToString());
        }

        [Fact]
        public void Logout_DeletesCookie_AndReturnsOk()
        {
            var mockCookies = new Mock<IRequestCookieCollection>();
            mockCookies.Setup(c => c["refreshToken"]).Returns("rt");

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Cookies = mockCookies.Object;
            _controller.ControllerContext.HttpContext = httpContext;

            var result = _controller.Logout();

            var ok = Assert.IsType<OkObjectResult>(result);

            Assert.True(httpContext.Response.Headers.ContainsKey("Set-Cookie"));
            Assert.Contains("refreshToken", httpContext.Response.Headers["Set-Cookie"].ToString());
            Assert.Contains("expires=Thu, 01 Jan 1970", httpContext.Response.Headers["Set-Cookie"].ToString());

            Assert.Contains("Logged out", ok.Value!.ToString());
        }

        [Fact]
        public async Task GetUsersCount_ReturnsOk()
        {
            _userServiceMock.Setup(s => s.GetAllUsersCountAsync()).ReturnsAsync(5);

            var result = await _controller.GetUsersCount();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(5, ok.Value);
        }

        [Fact]
        public async Task GetOldestCreatedUser_ReturnsOk()
        {
            var date = DateTime.UtcNow;
            _userServiceMock.Setup(s => s.GetOldestUserCreatedDateAsync()).ReturnsAsync(date);

            var result = await _controller.GetOldestCreatedUser();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(date, ok.Value);
        }

        [Fact]
        public async Task AddUserLike_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.AddUserLike(new UserLikes());

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Contains("Invalid token", unauthorized.Value.ToString());
        }

        [Fact]
        public async Task AddUserLike_ReturnsNotFound_WhenVehicleNull()
        {
            _controller.SetUserContextValid(true, 1);
            _vehicleServiceMock.Setup(s => s.GetVehicleByVinAsync("vin")).ReturnsAsync((Vehicle)null);

            var result = await _controller.AddUserLike(new UserLikes { vehicleVin = "vin" });

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task AddUserLike_ReturnsOk_WhenSuccess()
        {
            _controller.SetUserContextValid(true, 1);
            var userLikes = new UserLikes { vehicleVin = "vin" };
            var vehicle = new Vehicle();
            _vehicleServiceMock.Setup(s => s.GetVehicleByVinAsync("vin")).ReturnsAsync(vehicle);
            _userServiceMock.Setup(s => s.AddUserLikeAsync(It.Is<UserLikes>(ul => ul.userId == 1))).ReturnsAsync(userLikes);

            var result = await _controller.AddUserLike(userLikes);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(userLikes, ok.Value);
        }

        [Fact]
        public async Task RemoveUserLike_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.RemoveUserLike(new UserLikes());

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Contains("Invalid token", unauthorized.Value.ToString());
        }

        [Fact]
        public async Task RemoveUserLike_ReturnsNotFound_WhenVehicleNull()
        {
            _controller.SetUserContextValid(true, 1);
            _vehicleServiceMock.Setup(s => s.GetVehicleByVinAsync("vin")).ReturnsAsync((Vehicle)null);

            var result = await _controller.RemoveUserLike(new UserLikes { vehicleVin = "vin" });

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task RemoveUserLike_ReturnsOk_WhenSuccess()
        {
            _controller.SetUserContextValid(true, 1);
            var userLikes = new UserLikes { vehicleVin = "vin" };
            var vehicle = new Vehicle();
            _vehicleServiceMock.Setup(s => s.GetVehicleByVinAsync("vin")).ReturnsAsync(vehicle);
            _userServiceMock.Setup(s => s.RemoveUserLikeAsync(It.Is<UserLikes>(ul => ul.userId == 1))).ReturnsAsync(userLikes);

            var result = await _controller.RemoveUserLike(userLikes);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(userLikes, ok.Value);
        }

        [Fact]
        public async Task GetUserLikedVins_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.GetUserLikedVins();

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Contains("Invalid token", unauthorized.Value.ToString());
        }

        [Fact]
        public async Task GetUserLikedVins_ReturnsOk_WhenSuccess()
        {
            _controller.SetUserContextValid(true, 1);
            var vins = new List<string> { "vin1", "vin2" };
            _userServiceMock.Setup(s => s.GetUserLikedVinsAsync(1)).ReturnsAsync(vins);

            var result = await _controller.GetUserLikedVins();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(vins, ok.Value);
        }

        [Fact]
        public async Task GetUserSearches_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.GetUserSearches();

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Contains("Invalid token", unauthorized.Value.ToString());
        }

        [Fact]
        public async Task GetUserSearches_ReturnsOk_WhenSuccess()
        {
            _controller.SetUserContextValid(true, 1);
            var searches = new List<string> { "s1", "s2" };
            _userServiceMock.Setup(s => s.GetUserSavedSearches(1)).ReturnsAsync(searches);

            var result = await _controller.GetUserSearches();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(searches, ok.Value);
        }

        [Fact]
        public async Task DeleteUserSearch_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.DeleteUserSearch(new UserSavedSearch());

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Contains("Invalid token", unauthorized.Value.ToString());
        }

        [Fact]
        public async Task DeleteUserSearch_ReturnsForbid_WhenUserIdMismatch()
        {
            _controller.SetUserContextValid(true, 1);

            var result = await _controller.DeleteUserSearch(new UserSavedSearch { userId = 2 });

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task DeleteUserSearch_ReturnsNotFound_WhenNull()
        {
            _controller.SetUserContextValid(true, 1);
            var search = new UserSavedSearch { userId = 1, search = "s" };
            _userServiceMock.Setup(s => s.RemoveSavedSearchAsync(search)).ReturnsAsync((UserSavedSearch)null);

            var result = await _controller.DeleteUserSearch(search);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task DeleteUserSearch_ReturnsOk_WhenSuccess()
        {
            _controller.SetUserContextValid(true, 1);
            var search = new UserSavedSearch { userId = 1, search = "s" };
            _userServiceMock.Setup(s => s.RemoveSavedSearchAsync(search)).ReturnsAsync(search);

            var result = await _controller.DeleteUserSearch(search);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(search, ok.Value);
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenNull()
        {
            _userServiceMock.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync((User)null);

            var result = await _controller.GetUserById(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task GetUserById_ReturnsOk_WhenSuccess()
        {
            var user = new User { Id = 1 };
            _userServiceMock.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(user);

            var result = await _controller.GetUserById(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(user, ok.Value);
        }

        [Fact]
        public async Task SaveUserSearch_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.SaveUserSearch(new UserSavedSearch());

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Contains("Invalid token", unauthorized.Value.ToString());
        }

        [Fact]
        public async Task SaveUserSearch_ReturnsForbid_WhenUserIdMismatch()
        {
            _controller.SetUserContextValid(true, 1);

            var result = await _controller.SaveUserSearch(new UserSavedSearch { userId = 2 });

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task SaveUserSearch_ReturnsOk_WhenSuccess()
        {
            _controller.SetUserContextValid(true, 1);
            var search = new UserSavedSearch { userId = 1, search = "s" };
            _userServiceMock.Setup(s => s.AddUserSearchAsync(search)).ReturnsAsync(search);

            var result = await _controller.SaveUserSearch(search);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(search, ok.Value);
        }

        [Fact]
        public async Task AddUserInteraction_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.AddUserInteraction(new UserInteractions());

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Contains("Invalid token", unauthorized.Value.ToString());
        }

        [Fact]
        public async Task AddUserInteraction_ReturnsForbid_WhenUserIdMismatch()
        {
            _controller.SetUserContextValid(true, 1);

            var result = await _controller.AddUserInteraction(new UserInteractions { UserId = 2 });

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task AddUserInteraction_ReturnsNotFound_WhenVehicleNull()
        {
            _controller.SetUserContextValid(true, 1);
            var interaction = new UserInteractions { UserId = 1, VehicleId = 5 };
            _vehicleServiceMock.Setup(s => s.GetVehicleByIdAsync(5)).ReturnsAsync((Vehicle)null);

            var result = await _controller.AddUserInteraction(interaction);

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task AddUserInteraction_ReturnsOk_WhenSuccess()
        {
            _controller.SetUserContextValid(true, 1);
            var interaction = new UserInteractions { UserId = 1, VehicleId = 5, InteractionType = "view" };
            var vehicle = new Vehicle();
            var saved = new UserInteractions { Id = 10, UserId = 1, VehicleId = 5, InteractionType = "view", CreatedAt = DateTime.UtcNow };
            _vehicleServiceMock.Setup(s => s.GetVehicleByIdAsync(5)).ReturnsAsync(vehicle);
            _userServiceMock.Setup(s => s.AddUserInteractionAsync(interaction)).ReturnsAsync(saved);

            var result = await _controller.AddUserInteraction(interaction);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<UserInteractionsDTO>(ok.Value);
            Assert.Equal(saved.Id, dto.Id);
            Assert.Equal(saved.UserId, dto.UserId);
            Assert.Equal(saved.VehicleId, dto.VehicleId);
            Assert.Equal(saved.InteractionType, dto.InteractionType);
        }

        [Fact]
        public async Task Refresh_ReturnsUnauthorized_WhenNoCookie()
        {
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext = httpContext;

            var result = await _controller.Refresh();

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Refresh_ReturnsUnauthorized_WhenTokenNotFound()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Cookie"] = "refreshToken=rt";
            _controller.ControllerContext.HttpContext = httpContext;
            _refreshTokenServiceMock.Setup(s => s.GetAsync("rt")).ReturnsAsync((RefreshToken)null);

            var result = await _controller.Refresh();

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Refresh_ReturnsUnauthorized_WhenTokenExpiredOrRevoked()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Cookie"] = "refreshToken=rt";
            _controller.ControllerContext.HttpContext = httpContext;
            var token = new RefreshToken { Expires = DateTime.UtcNow.AddDays(-1), IsRevoked = false };
            _refreshTokenServiceMock.Setup(s => s.GetAsync("rt")).ReturnsAsync(token);

            var result = await _controller.Refresh();

            Assert.IsType<UnauthorizedResult>(result);

            token = new RefreshToken { Expires = DateTime.UtcNow.AddDays(1), IsRevoked = true };
            _refreshTokenServiceMock.Setup(s => s.GetAsync("rt")).ReturnsAsync(token);

            result = await _controller.Refresh();

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Refresh_ReturnsUnauthorized_WhenUserNotFound()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Cookie"] = "refreshToken=rt";
            _controller.ControllerContext.HttpContext = httpContext;
            var token = new RefreshToken { Expires = DateTime.UtcNow.AddDays(1), IsRevoked = false, UserId = 1 };
            _refreshTokenServiceMock.Setup(s => s.GetAsync("rt")).ReturnsAsync(token);
            _userServiceMock.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync((User)null!);

            var result = await _controller.Refresh();

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Refresh_ReturnsOk_WhenSuccess()
        {
            var httpContext = new DefaultHttpContext();
            var cookiesMock = new Mock<IRequestCookieCollection>();
            cookiesMock.Setup(c => c["refreshToken"]).Returns("rt");
            httpContext.Request.Cookies = cookiesMock.Object;

            _controller.ControllerContext.HttpContext = httpContext;

            var token = new RefreshToken { Expires = DateTime.UtcNow.AddDays(1), IsRevoked = false, UserId = 1 };
            var user = new User { Id = 1, Name = "n", Email = "e" };

            _refreshTokenServiceMock.Setup(s => s.GetAsync("rt")).ReturnsAsync(token);
            _userServiceMock.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(user);
            _tokenProviderMock.Setup(s => s.CreateAccessToken(user)).Returns("access");
            _tokenProviderMock.Setup(s => s.GenerateRefreshToken()).Returns("newrt");
            _refreshTokenServiceMock.Setup(s => s.RotateAsync(token, "newrt")).Returns(Task.CompletedTask);

            var result = await _controller.Refresh();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<AuthResponse>(okResult.Value);

            Assert.Equal("access", payload.AccessToken);
            Assert.Equal(user.Id, payload.UserId);
            Assert.Equal(user.Name, payload.UserName);
            Assert.Equal(user.Email, payload.UserEmail);

            var setCookieHeader = httpContext.Response.Headers["Set-Cookie"].ToString();
            Assert.Contains("refreshToken", setCookieHeader);
            Assert.Contains("HttpOnly", setCookieHeader);
            Assert.Contains("Secure", setCookieHeader);
        }

        private class TestUserController : UserController
        {
            private bool _isUserContextValid;
            private int _userId;

            public TestUserController(
                IUserService userService,
                IVehicleService vehicleService,
                ITokenProvider tokenProvider,
                IRefreshTokenService refreshTokenService,
                ILogger<UserController> logger)
                : base(userService, vehicleService, tokenProvider, refreshTokenService, logger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };
            }

            public void SetUserContextValid(bool valid, int userId)
            {
                _isUserContextValid = valid;
                _userId = userId;
            }

            protected override bool IsUserContextValid(out int userId)
            {
                userId = _userId;
                return _isUserContextValid;
            }

        }
    }
}