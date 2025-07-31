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
    [ApiController]
    [Route("[controller]")]

    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IVehicleService _vehicleService;

        public UserController(IUserService userService, IVehicleService vehicleService)
        {
            _userService = userService;
            _vehicleService = vehicleService;
        }
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

        [Authorize]
        [HttpPost("add-user-like")]
        public async Task<ActionResult<UserLikes>> AddUserLike([FromBody] UserLikes userLikes)
        {
            var user = await _userService.GetUserByIdAsync(userLikes.userId);
            var vehicle = await _vehicleService.GetVehicleByVinAsync(userLikes.vehicleVin);

            if (user == null)
            {
                return NotFound(new { message = $"User with ID {userLikes.userId} not found." });
            }

            if (vehicle == null)
            {
                return NotFound(new { message = $"Vehicle with VIN {userLikes.vehicleVin} not found." });
            }

            var addedLike = await _userService.AddUserLikeAsync(userLikes);
            return Ok(addedLike);
        }

        [Authorize]
        [HttpDelete("remove-user-like")]
        public async Task<ActionResult<UserLikes>> RemoveUserLike([FromBody] UserLikes userLikes)
        {
            var user = await _userService.GetUserByIdAsync(userLikes.userId);
            var vehicle = await _vehicleService.GetVehicleByVinAsync(userLikes.vehicleVin);

            if (user == null)
            {
                return NotFound(new { message = $"User with ID {userLikes.userId} not found." });
            }

            if (vehicle == null)
            {
                return NotFound(new { message = $"Vehicle with VIN {userLikes.vehicleVin} not found." });
            }

            var removedLike = await _userService.RemoveUserLikeAsync(userLikes);
            return Ok(removedLike);
        }

        [Authorize]
        [HttpGet("get-user-liked-vins")]
        public async Task<ActionResult<List<string>>> GetUserLikedVins()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                        User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found");
            }
            var vins = await _userService.GetUserLikedVinsAsync(userId);
            return Ok(vins);
        }

        [Authorize]
        [HttpGet("get-user-saved-searches")]
        public async Task<ActionResult<List<string>>> GetUserSearches()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                         User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found");
            }
            var searches = await _userService.GetUserSavedSearches(userId);
            return Ok(searches);
        }

        [Authorize]
        [HttpDelete("delete-search")]
        public async Task<ActionResult<UserSavedSearch>> DeleteUserSearch([FromBody] UserSavedSearch search)
        {
            var user = await _userService.GetUserByIdAsync(search.userId);
            if (user == null)
                return NotFound($"User with ID {search.userId} not found");


            var savedSearch = await _userService.RemoveSavedSearchAsync(search);
            if (savedSearch == null)
                return NotFound($"Search {search.search} with User ID {search.userId} not found");
            return Ok(savedSearch);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound($"User with ID {id} not found");
            return Ok(user);
        }

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
