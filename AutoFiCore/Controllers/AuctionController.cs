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
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuctionController"/> class.
        /// </summary>
        /// <param name="auctionService">Service for handling auction operations.</param>
        public AuctionController(IAuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        //[Authorize(Roles = "Admin")]
        /// <summary>
        /// Creates a new auction.
        /// </summary>
        /// <param name="dto">Auction creation data.</param>
        /// <returns>Returns the created auction or an error.</returns>
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
            var result = await _auctionService.PlaceBidAsync(auctionId, dto);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves bid history for a specific auction.
        /// </summary>
        /// <param name="auctionId">ID of the auction.</param>
        /// <returns>Returns the bid history or a not found error.</returns>
        [HttpGet("{id}/bids")]
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
        [HttpGet("userBids")]
        public async Task<IActionResult> GetUserBidHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                              User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var result = await _auctionService.GetUserBidHistoryAsync(userId);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                              User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var result = await _auctionService.AddToWatchListAsync(userId, id);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value!);
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                              User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var result = await _auctionService.RemoveFromWatchListAsync(userId, id);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the authenticated user's watchlist.
        /// </summary>
        /// <returns>Returns the user's watchlist or a not found error.</returns>
        [Authorize]
        [HttpGet("user/watchlist")]
        public async Task<IActionResult> GetUserWatchlist()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                              User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var watchlists = await _auctionService.GetUserWatchListAsync(userId);

            if (!watchlists.IsSuccess)
                return NotFound(new { error = watchlists.Error });

            return Ok(watchlists.Value);
        }

        /// <summary>
        /// Retrieves all users watching a specific auction.
        /// </summary>
        /// <param name="auctionId">ID of the auction.</param>
        /// <returns>Returns a list of users watching the auction or a not found error.</returns>
        [HttpGet("{auctionId}/watchers")]
        public async Task<IActionResult> GetAuctionWatchers(int auctionId)
        {
            var watchlists = await _auctionService.GetAuctionWatchersAsync(auctionId);

            if (!watchlists.IsSuccess)
                return NotFound(new { error = watchlists.Error });

            return Ok(watchlists.Value);
        }

        /// <summary>
        /// Retrieves the highest bidder's user ID for a specific auction.
        /// </summary>
        /// <param name="auctionId">ID of the auction.</param>
        /// <returns>Returns the highest bidder's ID or a not found error.</returns>
        [HttpGet("highest-bidder/{auctionId}")]
        public async Task<IActionResult> GetHighestBidderId(int auctionId)
        {
            var highestId = await _auctionService.GetHighestBidderIdAsync(auctionId);

            if (!highestId.IsSuccess)
                return NotFound(new { error = highestId.Error });

            return Ok(highestId.Value);
        }

        /// <summary>
        /// Processes and retrieves the result of a specific auction.
        /// </summary>
        /// <param name="auctionId">ID of the auction.</param>
        /// <returns>Returns the auction result or an error.</returns>
        [HttpGet("{auctionId}/result")]
        public async Task<IActionResult> GetAuctionResult(int auctionId)
        {
            var result = await _auctionService.ProcessAuctionResultAsync(auctionId);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        /// <summary>
        /// Verifies if the authenticated user is authorized for auction checkout.
        /// </summary>
        /// <param name="id">ID of the auction.</param>
        /// <returns>Returns success if the user is authorized.</returns>
        [Authorize]
        [HttpGet("{id}/checkout")]
        public IActionResult VerifyCheckoutAccess(int id)
        {
            return Ok(new { success = true, message = "User authorized for auction checkout." });
        }
    }
}
