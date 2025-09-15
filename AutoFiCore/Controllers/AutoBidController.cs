using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using AutoFiCore.Data.Interfaces;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Handles operations related to auto-bidding functionality for auctions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AutoBidController : SecureControllerBase
    {
        private readonly IAutoBidService _autoBidService;
        private readonly ILogger<AutoBidController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoBidController"/> class.
        /// </summary>
        /// <param name="autoBidService">Service for managing auto-bid logic.</param>
        public AutoBidController(IAutoBidService autoBidService, ILogger<AutoBidController> logger)
        {
            _autoBidService = autoBidService;
            _logger = logger;
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
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("CreateAutoBid called. CorrelationId={CorrelationId}, UserId={UserId}, AuctionId={AuctionId}", correlationId, userId, dto.AuctionId);

            dto.UserId = userId;
            var result = await _autoBidService.CreateAutoBidAsync(dto);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
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
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("UpdateAutoBid called. CorrelationId={CorrelationId}, UserId={UserId}, AuctionId={AuctionId}", correlationId, userId, auctionId);

            var result = await _autoBidService.UpdateAutoBidAsync(auctionId, userId, dto);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
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
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("CancelAutoBid called. CorrelationId={CorrelationId}, UserId={UserId}, AuctionId={AuctionId}", correlationId, userId, auctionId);

            var result = await _autoBidService.CancelAutoBidAsync(auctionId, userId);
            return result.IsSuccess ? Ok(new { message = "Auto-bid cancelled successfully" }) : BadRequest(new { error = result.Error });
        }

        /// <summary>
        /// Checks whether an auto-bid is set for the current user on a specific auction.
        /// </summary>
        /// <param name="auctionId">Auction ID to check.</param>
        /// <returns>Returns true if auto-bid is set; otherwise false.</returns>
        [Authorize]
        [HttpGet("auction/{auctionId}")]
        public async Task<IActionResult> IsAutoBidSet(int auctionId)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("IsAutoBidSet called. CorrelationId={CorrelationId}, UserId={UserId}, AuctionId={AuctionId}", correlationId, userId, auctionId);

            var result = await _autoBidService.IsAutoBidSetAsync(auctionId, userId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
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
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("GetAutoBidWithStrategy called. CorrelationId={CorrelationId}, UserId={UserId}, AuctionId={AuctionId}", correlationId, userId, auctionId);

            var result = await _autoBidService.GetAutoBidWithStrategyAsync(userId, auctionId);
            return result.IsSuccess ? Ok(result.Value) : NotFound(new { message = result.Error });
        }

        /// <summary>
        /// Retrieves a summary of auto-bid activity for a specific auction.
        /// </summary>
        /// <param name="auctionId">Auction ID to summarize.</param>
        /// <returns>Returns auto-bid summary or not found message.</returns>
        [AllowAnonymous]
        [HttpGet("/api/auction/{auctionId}/autobids")]
        public async Task<IActionResult> GetAuctionAutoBidSummary(int auctionId)
        {
            var summary = await _autoBidService.GetAuctionAutoBidSummaryAsync(auctionId);
            if (summary == null)
                return NotFound($"Auction with ID {auctionId} not found.");

            return Ok(summary.Value);
        }
    }
}