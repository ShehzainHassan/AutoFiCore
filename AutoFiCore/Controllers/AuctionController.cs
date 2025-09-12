using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Mappers;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Controller for managing auction-related operations such as creating auctions, placing bids, and managing watchlists.
    /// </summary>
    [ApiController]
    [Route("auction")]
    public class AuctionController : SecureControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly ILogger<AuctionController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuctionController"/> class.
        /// </summary>
        /// <param name="auctionService">Service for handling auction operations.</param>
        public AuctionController(IAuctionService auctionService, ILogger<AuctionController> logger)
        {
            _auctionService = auctionService;
            _logger = logger;
        }

        //[Authorize(Roles = "Admin")]
        /// <summary>
        /// Creates a new auction.
        /// </summary>
        /// <param name="dto">Auction creation data.</param>
        /// <returns>Returns the created auction or an error.</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDTO dto)
        {
            var result = await _auctionService.CreateAuctionAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            var auction = result.Value!;
            return Ok(auction);
        }

        /// <summary>
        /// Updates the status of an existing auction.
        /// </summary>
        /// <param name="id">ID of the auction to update.</param>
        /// <param name="dto">Data containing the new status.</param>
        /// <returns>Returns the updated auction status or an error.</returns>
        [AllowAnonymous]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAuctionStatusDTO dto)
        {
            var result = await _auctionService.UpdateAuctionStatusAsync(id, dto.Status);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves all auctions with optional filters for status, make, and price range.
        /// </summary>
        /// <param name="filters">Optional filters for querying auctions.</param>
        /// <remarks>
        /// Example query:
        /// <c>GET /auction?status=Active&amp;make=Toyota&amp;minPrice=10000&amp;maxPrice=50000&amp;sortBy=price&amp;descending=true</c>
        /// </remarks>        
        /// <returns>Returns a list of auctions matching the specified filters.</returns>
        /// <response code="200">Returns the list of auctions.</response>
        /// <response code="400">If the request parameters are invalid.</response>
        [AllowAnonymous]
        [HttpGet]
        [DisableRateLimiting]
        public async Task<IActionResult> GetAuctions([FromQuery] AuctionQueryParams filters)
        {
            var auctions = await _auctionService.GetAuctionsAsync(filters);
            return Ok(auctions);
        }

        /// <summary>
        /// Retrieves a specific auction by its ID.
        /// </summary>
        /// <param name="id">ID of the auction.</param>
        /// <returns>Returns the auction details or a not found error.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuction(int id)
        {
            var result = await _auctionService.GetAuctionByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the date of the oldest auction in the system.
        /// </summary>
        /// <returns>Returns the oldest auction date.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("oldest-auction")]
        public async Task<IActionResult> GetOldestAuctionDate()
        {
            var result = await _auctionService.GetOldestAuctionDateAsync();
            return Ok(result);
        }

        /// <summary>
        /// Places a bid on a specific auction.
        /// </summary>
        /// <param name="auctionId">ID of the auction to bid on.</param>
        /// <param name="dto">Bid creation data.</param>
        /// <returns>Returns the placed bid or an error.</returns>
        [Authorize]
        [DisableRateLimiting]
        [HttpPost("{id}/bids")]
        public async Task<IActionResult> PlaceBid([FromRoute(Name = "id")] int auctionId, [FromBody] CreateBidDTO dto)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { message = "Invalid token or user context." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("PlaceBid called. CorrelationId={CorrelationId}, UserId={UserId}, AuctionId={AuctionId}, Amount={Amount}",
                correlationId, userId, auctionId, dto.Amount);

            dto.UserId = userId;
            var result = await _auctionService.PlaceBidAsync(auctionId, dto);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }

        /// <summary>
        /// Retrieves bid history for a specific auction.
        /// </summary>
        /// <param name="auctionId">ID of the auction.</param>
        /// <returns>Returns the bid history or a not found error.</returns>
        [HttpGet("{id}/bids")]
        [DisableRateLimiting]
        public async Task<IActionResult> GetBidHistory([FromRoute(Name = "id")] int auctionId)
        {
            var result = await _auctionService.GetBidHistoryAsync(auctionId);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the bid history for the currently authenticated user.
        /// </summary>
        /// <returns>Returns the user's bid history or a not found error.</returns>

        [Authorize]
        [DisableRateLimiting]
        [HttpGet("userBids")]
        public async Task<IActionResult> GetUserBidHistory()
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("GetUserBidHistory called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var result = await _auctionService.GetUserBidHistoryAsync(userId);
            return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
        }


        /// <summary>
        /// Adds an auction to the authenticated user's watchlist.
        /// </summary>
        /// <param name="id">ID of the auction to add.</param>
        /// <returns>Returns the updated watchlist or an error.</returns>
        [Authorize]
        [HttpPost("{id}/watch")]
        public async Task<IActionResult> AddToWatchlist(int id)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("AddToWatchlist called. CorrelationId={CorrelationId}, UserId={UserId}, AuctionId={AuctionId}", correlationId, userId, id);

            var result = await _auctionService.AddToWatchListAsync(userId, id);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }


        /// <summary>
        /// Removes an auction from the authenticated user's watchlist.
        /// </summary>
        /// <param name="id">ID of the auction to remove.</param>
        /// <returns>Returns the updated watchlist or an error.</returns>
        [Authorize]
        [HttpDelete("{id}/watch")]
        public async Task<IActionResult> RemoveFromWatchlist(int id)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("RemoveFromWatchlist called. CorrelationId={CorrelationId}, UserId={UserId}, AuctionId={AuctionId}", correlationId, userId, id);

            var result = await _auctionService.RemoveFromWatchListAsync(userId, id);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
        }


        /// <summary>
        /// Retrieves the authenticated user's watchlist.
        /// </summary>
        /// <returns>Returns the user's watchlist or a not found error.</returns>
        [Authorize]
        [DisableRateLimiting]
        [HttpGet("user/watchlist")]
        public async Task<IActionResult> GetUserWatchlist()
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("GetUserWatchlist called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var result = await _auctionService.GetUserWatchListAsync(userId);
            return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
        }


        /// <summary>
        /// Retrieves all users watching a specific auction.
        /// </summary>
        /// <param name="auctionId">ID of the auction.</param>
        /// <returns>Returns a list of users watching the auction or a not found error.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("{auctionId}/watchers")]
        public async Task<IActionResult> GetAuctionWatchers(int auctionId)
        {
            var correlationId = GetCorrelationId();
            _logger.LogInformation("GetAuctionWatchers called. CorrelationId={CorrelationId}, AuctionId={AuctionId}", correlationId, auctionId);

            var result = await _auctionService.GetAuctionWatchersAsync(auctionId);
            return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
        }


        /// <summary>
        /// Retrieves the highest bidder's user ID for a specific auction.
        /// </summary>
        /// <param name="auctionId">ID of the auction.</param>
        /// <returns>Returns the highest bidder's ID or a not found error.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("highest-bidder/{auctionId}")]
        public async Task<IActionResult> GetHighestBidderId(int auctionId)
        {
            var correlationId = GetCorrelationId();
            _logger.LogInformation("GetHighestBidderId called. CorrelationId={CorrelationId}, AuctionId={AuctionId}", correlationId, auctionId);

            var result = await _auctionService.GetHighestBidderIdAsync(auctionId);
            return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
        }


        /// <summary>
        /// Processes and retrieves the result of a specific auction.
        /// </summary>
        /// <param name="auctionId">ID of the auction.</param>
        /// <returns>Returns the auction result or an error.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("{auctionId}/result")]
        public async Task<IActionResult> GetAuctionResult(int auctionId)
        {
            var correlationId = GetCorrelationId();
            _logger.LogInformation("GetAuctionResult called. CorrelationId={CorrelationId}, AuctionId={AuctionId}", correlationId, auctionId);

            var result = await _auctionService.ProcessAuctionResultAsync(auctionId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }


        /// <summary>
        /// Verifies if the authenticated user is authorized for auction checkout.
        /// </summary>
        /// <param name="id">ID of the auction.</param>
        /// <returns>Returns success if the user is authorized.</returns>
        [Authorize]
        [DisableRateLimiting]
        [HttpGet("{id}/checkout")]
        public IActionResult VerifyCheckoutAccess(int id)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("VerifyCheckoutAccess called. CorrelationId={CorrelationId}, UserId={UserId}, AuctionId={AuctionId}", correlationId, userId, id);

            return Ok(new { success = true, message = "User authorized for auction checkout." });
        }

    }
}
