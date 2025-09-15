using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Controller for managing user-related operations such as creating users, logging in, likes, saved searches, and interactions.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UserController : SecureControllerBase
    {
        private readonly IUserService _userService;
        private readonly IVehicleService _vehicleService;
        private readonly ITokenProvider _tokenProvider;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userService">Service for handling user-related operations.</param>
        /// <param name="vehicleService">Service for handling vehicle-related operations.</param>
        public UserController(IUserService userService, IVehicleService vehicleService, ITokenProvider tokenProvider, IRefreshTokenService refreshTokenService, ILogger<UserController> logger)
        {
            _userService = userService;
            _vehicleService = vehicleService;
            _tokenProvider = tokenProvider;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new user in the system.
        /// </summary>
        /// <param name="user">The user entity to create.</param>
        /// <returns>Returns the created user or error details.</returns>
        [AllowAnonymous]
        [HttpPost("add")]
        public async Task<ActionResult> CreateUser([FromBody] User user)
        {
            var result = await _userService.AddUserAsync(user);

            if (!result.IsSuccess)
            {
                if (result.Errors != null && result.Errors.Any())
                    return BadRequest(new { errors = result.Errors });

                if (!string.IsNullOrEmpty(result.Error))
                    return Conflict(new { message = result.Error });
            }

            return Ok(result.Value);
        }


        /// <summary>
        /// Authenticates a user with email and password.
        /// </summary>
        /// <param name="loginDTO">The login credentials.</param>
        /// <returns>Returns the authenticated user or unauthorized error.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpPost("login")]
        public async Task<ActionResult<User>> LoginUser([FromBody] LoginDTO loginDTO)
        {
            var response = await _userService.LoginUserAsync(loginDTO.Email, loginDTO.Password);
            if (response == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var refreshToken = await _refreshTokenService.GetLatestTokenForUserAsync(response.UserId);
            Response.Cookies.Append("refreshToken", refreshToken!.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.Expires
            });

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Logged out successfully" });
        }
        /// <summary>
        /// Gets the total count of all users.
        /// </summary>
        /// <returns>Returns the count of users.</returns>
        [AllowAnonymous]
        [HttpGet("all-users-count")]
        public async Task<ActionResult<UserLikes>> GetUsersCount()
        {
            var count = await _userService.GetAllUsersCountAsync();
            return Ok(count);
        }

        /// <summary>
        /// Gets the creation date of the oldest user in the system.
        /// </summary>
        /// <returns>Returns the date of the oldest user creation.</returns>
        [AllowAnonymous]
        [HttpGet("oldest-user")]
        public async Task<ActionResult> GetOldestCreatedUser()
        {
            var date = await _userService.GetOldestUserCreatedDateAsync();
            return Ok(date);
        }

        /// <summary>
        /// Adds a like for a vehicle by a user.
        /// </summary>
        /// <param name="userLikes">User like information.</param>
        /// <returns>Returns the added like or error if user or vehicle not found.</returns>
        [Authorize]
        [HttpPost("add-user-like")]
        public async Task<ActionResult<UserLikes>> AddUserLike([FromBody] UserLikes userLikes)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { message = "Invalid token or user context. " });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("AddUserLike called. CorrelationId={CorrelationId}, UserId={UserId}, VIN={Vin}", correlationId, userId, userLikes.vehicleVin);

            var vehicle = await _vehicleService.GetVehicleByVinAsync(userLikes.vehicleVin);
            if (vehicle == null)
                return NotFound(new { message = $"Vehicle with VIN {userLikes.vehicleVin} not found." });

            userLikes.userId = userId;
            var addedLike = await _userService.AddUserLikeAsync(userLikes);
            return Ok(addedLike);
        }

        /// <summary>
        /// Removes a like for a vehicle by a user.
        /// </summary>
        /// <param name="userLikes">User like information.</param>
        /// <returns>Returns the removed like or error if user or vehicle not found.</returns>
        [Authorize]
        [HttpDelete("remove-user-like")]
        public async Task<ActionResult<UserLikes>> RemoveUserLike([FromBody] UserLikes userLikes)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { message = "Invalid token or user context." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("RemoveUserLike called. CorrelationId={CorrelationId}, UserId={UserId}, VIN={Vin}", correlationId, userId, userLikes.vehicleVin);

            var vehicle = await _vehicleService.GetVehicleByVinAsync(userLikes.vehicleVin);
            if (vehicle == null)
                return NotFound(new { message = $"Vehicle with VIN {userLikes.vehicleVin} not found." });

            userLikes.userId = userId;
            var removedLike = await _userService.RemoveUserLikeAsync(userLikes);
            return Ok(removedLike);
        }

        /// <summary>
        /// Retrieves all vehicle VINs liked by the authenticated user.
        /// </summary>
        /// <returns>Returns a list of vehicle VINs liked by the user.</returns>
        [Authorize]
        [DisableRateLimiting]
        [HttpGet("get-user-liked-vins")]
        public async Task<ActionResult<List<string>>> GetUserLikedVins()
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { message = "Invalid token or user context." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("GetUserLikedVins called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var vins = await _userService.GetUserLikedVinsAsync(userId);
            return Ok(vins);
        }

        [Authorize]
        [DisableRateLimiting]
        [HttpGet("get-user-saved-searches")]
        public async Task<ActionResult<List<string>>> GetUserSearches()
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { message = "Invalid token or user context." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("GetUserSearches called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var searches = await _userService.GetUserSavedSearches(userId);
            return Ok(searches);
        }

        [Authorize]
        [HttpDelete("delete-search")]
        public async Task<ActionResult<UserSavedSearch>> DeleteUserSearch([FromBody] UserSavedSearch search)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { message = "Invalid token or user context." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("DeleteUserSearch called. CorrelationId={CorrelationId}, UserId={UserId}, Search={Search}", correlationId, userId, search.search);

            if (search.userId != userId)
                return Forbid("User ID mismatch between token and payload.");

            var savedSearch = await _userService.RemoveSavedSearchAsync(search);
            if (savedSearch == null)
                return NotFound($"Search '{search.search}' with User ID {search.userId} not found");

            return Ok(savedSearch);
        }
        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        /// <param name="id">The user's ID.</param>
        /// <returns>Returns the user details or not found error.</returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound($"User with ID {id} not found");

            return Ok(user);
        }

        /// <summary>
        /// Saves a new search for a user.
        /// </summary>
        /// <param name="search">The search to save.</param>
        /// <returns>Returns the saved search.</returns>
        [Authorize]
        [HttpPost("save-search")]
        public async Task<ActionResult<UserSavedSearch>> SaveUserSearch([FromBody] UserSavedSearch search)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { message = "Invalid token or user context." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("SaveUserSearch called. CorrelationId={CorrelationId}, UserId={UserId}, Search={Search}", correlationId, userId, search.search);

            if (search.userId != userId)
                return Forbid("User ID mismatch between token and payload.");

            var savedSearch = await _userService.AddUserSearchAsync(search);
            return Ok(savedSearch);
        }

        /// <summary>
        /// Adds a new interaction for a user with a vehicle.
        /// </summary>
        /// <param name="userInteraction">The interaction details.</param>
        /// <returns>Returns the added interaction.</returns>

        [Authorize]
        [HttpPost("add-interaction")]
        public async Task<ActionResult<UserInteractionsDTO>> AddUserInteraction([FromBody] UserInteractions userInteraction)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { message = "Invalid token or user context." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("AddUserInteraction called. CorrelationId={CorrelationId}, UserId={UserId}, VehicleId={VehicleId}, Type={Type}",
                correlationId, userId, userInteraction.VehicleId, userInteraction.InteractionType);

            if (userInteraction.UserId != userId)
                return Forbid("User ID mismatch between token and payload.");

            var vehicle = await _vehicleService.GetVehicleByIdAsync(userInteraction.VehicleId);
            if (vehicle == null)
                return NotFound($"Vehicle with ID {userInteraction.VehicleId} not found");

            var savedInteraction = await _userService.AddUserInteractionAsync(userInteraction);
            return Ok(new UserInteractionsDTO
            {
                Id = savedInteraction.Id,
                UserId = savedInteraction.UserId,
                VehicleId = savedInteraction.VehicleId,
                InteractionType = savedInteraction.InteractionType,
                CreatedAt = savedInteraction.CreatedAt
            });
        }

        [DisableRateLimiting]
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var stored = await _refreshTokenService.GetAsync(refreshToken);
            if (stored == null || stored.Expires < DateTime.UtcNow || stored.IsRevoked)
                return Unauthorized();

            var user = await _userService.GetUserByIdAsync(stored.UserId);
            if (user == null) return Unauthorized();

            var newAccessToken = _tokenProvider.CreateAccessToken(user);

            if (stored.Expires < DateTime.UtcNow.AddDays(2))
            {
                var newRefreshToken = _tokenProvider.GenerateRefreshToken();
                await _refreshTokenService.RotateAsync(stored, newRefreshToken);

                Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });
            }

            return Ok(new AuthResponse
            {
                AccessToken = newAccessToken,
                UserId = user.Id,
                UserName = user.Name,
                UserEmail = user.Email
            });
        }

    }
}
