using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Controller for managing user-related operations such as creating users, logging in, likes, saved searches, and interactions.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IVehicleService _vehicleService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userService">Service for handling user-related operations.</param>
        /// <param name="vehicleService">Service for handling vehicle-related operations.</param>
        public UserController(IUserService userService, IVehicleService vehicleService)
        {
            _userService = userService;
            _vehicleService = vehicleService;
        }

        /// <summary>
        /// Creates a new user in the system.
        /// </summary>
        /// <param name="user">The user entity to create.</param>
        /// <returns>Returns the created user or error details.</returns>
        [HttpPost("add")]
        public async Task<ActionResult> CreateUser([FromBody] User user)
        {
            var result = await _userService.AddUserAsync(user);

            if (!result.IsSuccess)
            {
                if (result.Errors.Any())
                    return BadRequest(new { errors = result.Errors });

                return Conflict(new { message = result.Error });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Authenticates a user with email and password.
        /// </summary>
        /// <param name="loginDTO">The login credentials.</param>
        /// <returns>Returns the authenticated user or unauthorized error.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<User>> LoginUser([FromBody] LoginDTO loginDTO)
        {
            var user = await _userService.LoginUserAsync(loginDTO.Email, loginDTO.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }
            return Ok(user);
        }

        /// <summary>
        /// Gets the total count of all users.
        /// </summary>
        /// <returns>Returns the count of users.</returns>
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
        [HttpGet("oldest-user")]
        public async Task<ActionResult> GetOldestCreatedUsre()
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
            var user = await _userService.GetUserByIdAsync(userLikes.userId);
            var vehicle = await _vehicleService.GetVehicleByVinAsync(userLikes.vehicleVin);

            if (user == null)
                return NotFound(new { message = $"User with ID {userLikes.userId} not found." });

            if (vehicle == null)
                return NotFound(new { message = $"Vehicle with VIN {userLikes.vehicleVin} not found." });

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
            var user = await _userService.GetUserByIdAsync(userLikes.userId);
            var vehicle = await _vehicleService.GetVehicleByVinAsync(userLikes.vehicleVin);

            if (user == null)
                return NotFound(new { message = $"User with ID {userLikes.userId} not found." });

            if (vehicle == null)
                return NotFound(new { message = $"Vehicle with VIN {userLikes.vehicleVin} not found." });

            var removedLike = await _userService.RemoveUserLikeAsync(userLikes);
            return Ok(removedLike);
        }

        /// <summary>
        /// Retrieves all vehicle VINs liked by the authenticated user.
        /// </summary>
        /// <returns>Returns a list of vehicle VINs liked by the user.</returns>
        [Authorize]
        [HttpGet("get-user-liked-vins")]
        public async Task<ActionResult<List<string>>> GetUserLikedVins()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                        User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound($"User with ID {userId} not found");

            var vins = await _userService.GetUserLikedVinsAsync(userId);
            return Ok(vins);
        }

        /// <summary>
        /// Retrieves all saved searches for the authenticated user.
        /// </summary>
        /// <returns>Returns a list of saved searches.</returns>
        [Authorize]
        [HttpGet("get-user-saved-searches")]
        public async Task<ActionResult<List<string>>> GetUserSearches()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                         User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound($"User with ID {userId} not found");

            var searches = await _userService.GetUserSavedSearches(userId);
            return Ok(searches);
        }

        /// <summary>
        /// Deletes a saved search for a user.
        /// </summary>
        /// <param name="search">The saved search to delete.</param>
        /// <returns>Returns the deleted search or not found error.</returns>
        [Authorize]
        [HttpDelete("delete-search")]
        public async Task<ActionResult<UserSavedSearch>> DeleteUserSearch([FromBody] UserSavedSearch search)
        {
            var user = await _userService.GetUserByIdAsync(search.userId);
            if (user == null)
                return NotFound($"User with ID {search.userId} not found");

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
            var user = await _userService.GetUserByIdAsync(search.userId);
            if (user == null)
                return NotFound($"User with ID {search.userId} not found");

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
            var user = await _userService.GetUserByIdAsync(userInteraction.UserId);
            if (user == null)
                return NotFound($"User with ID {userInteraction.UserId} not found");

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
    }
}
