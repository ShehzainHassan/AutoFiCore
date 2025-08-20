using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Handles operations related to auto-bidding functionality for auctions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AutoBidController : ControllerBase
    {
        private readonly IAutoBidService _autoBidService;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoBidController"/> class.
        /// </summary>
        /// <param name="autoBidService">Service for managing auto-bid logic.</param>
        /// <param name="unitOfWork">Unit of work for transactional operations.</param>
        public AutoBidController(IAutoBidService autoBidService, IUnitOfWork unitOfWork)
        {
            _autoBidService = autoBidService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Creates a new auto-bid configuration for a user.
        /// </summary>
        /// <param name="dto">Auto-bid creation details.</param>
        /// <returns>Returns the created auto-bid configuration or an error message.</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAutoBid([FromBody] CreateAutoBidDTO dto)
        {
            var result = await _autoBidService.CreateAutoBidAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Updates an existing auto-bid configuration for a specific auction.
        /// </summary>
        /// <param name="auctionId">Auction ID to update auto-bid for.</param>
        /// <param name="dto">Updated auto-bid details.</param>
        /// <returns>Returns the updated auto-bid configuration or an error message.</returns>
        [Authorize]
        [HttpPut("update/auction/{auctionId}")]
        public async Task<IActionResult> UpdateAutoBid(int auctionId, [FromBody] UpdateAutoBidDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                              User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }

            var result = await _autoBidService.UpdateAutoBidAsync(auctionId, userId, dto);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Cancels the auto-bid configuration for a specific auction.
        /// </summary>
        /// <param name="auctionId">Auction ID to cancel auto-bid for.</param>
        /// <returns>Returns success message or error.</returns>
        [Authorize]
        [HttpDelete("{auctionId}")]
        public async Task<IActionResult> CancelAutoBid(int auctionId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                              User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }

            var result = await _autoBidService.CancelAutoBidAsync(auctionId, userId);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(new { message = "Auto-bid cancelled successfully" });
        }

        /// <summary>
        /// Checks whether an auto-bid is set for the current user on a specific auction.
        /// </summary>
        /// <param name="auctionId">Auction ID to check.</param>
        /// <returns>Returns true if auto-bid is set; otherwise false.</returns>
        [Authorize]
        [HttpGet("auction/{auctionId}")]
        public async Task<ActionResult<bool>> IsAutoBidSet(int auctionId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                              User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }

            var result = await _autoBidService.IsAutoBidSetAsync(auctionId, userId);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the auto-bid configuration and strategy for a specific auction.
        /// </summary>
        /// <param name="auctionId">Auction ID to retrieve strategy for.</param>
        /// <returns>Returns auto-bid strategy or not found message.</returns>
        [Authorize]
        [HttpGet("{auctionId}")]
        public async Task<IActionResult> GetAutoBidWithStrategy(int auctionId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                              User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }

            var result = await _autoBidService.GetAutoBidWithStrategyAsync(userId, auctionId);

            if (!result.IsSuccess)
                return NotFound(new { message = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves a summary of auto-bid activity for a specific auction.
        /// </summary>
        /// <param name="auctionId">Auction ID to summarize.</param>
        /// <returns>Returns auto-bid summary or not found message.</returns>
        [HttpGet("/api/auction/{auctionId}/autobids")]
        public async Task<ActionResult<AutoBidSummaryDto>> GetAuctionAutoBidSummary(int auctionId)
        {
            var summary = await _autoBidService.GetAuctionAutoBidSummaryAsync(auctionId);
            if (summary == null)
                return NotFound($"Auction with ID {auctionId} not found.");

            return Ok(summary);
        }
    }
}