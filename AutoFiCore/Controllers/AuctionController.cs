using AutoFiCore.Dto;
using AutoFiCore.Mappers;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AutoFiCore.Controllers
{
    [ApiController]
    [Route("auction")]
    public class AuctionController:ControllerBase
    {
        private readonly IAuctionService _auctionService;
        
        public AuctionController(IAuctionService auctionService)
        {
             _auctionService = auctionService;
        }
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDTO dto)
        {
            var result = await _auctionService.CreateAuctionAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            var auction = result.Value!;
            return Ok(auction);
        }

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
        /// GET /api/auctions?status=Active&make=Toyota&minPrice=10000&maxPrice=50000&sortBy=price&descending=true
        /// </remarks>
        /// <returns>Returns a list of auctions matching the specified filters.</returns>
        /// <response code="200">Returns the list of auctions</response>
        /// <response code="400">If the request parameters are invalid</response>
        [HttpGet]
        public async Task<IActionResult> GetAuctions([FromQuery] AuctionQueryParams filters)
        {
            var auctions = await _auctionService.GetAuctionsAsync(filters);
            return Ok(auctions);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuction(int id)
        {
            var result = await _auctionService.GetAuctionByIdAsync(id);
            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpGet("oldest-auction")]
        public async Task<IActionResult> GetOldestAuctionDate()
        {
            var result = await _auctionService.GetOldestAuctionDateAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpPost("{id}/bids")]
        public async Task<IActionResult> PlaceBid([FromRoute(Name = "id")] int auctionId, [FromBody] CreateBidDTO dto)
        {
            var result = await _auctionService.PlaceBidAsync(auctionId, dto);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }

        [HttpGet("{id}/bids")]
        public async Task<IActionResult> GetBidHistory([FromRoute(Name = "id")] int auctionId)
        {
            var result = await _auctionService.GetBidHistoryAsync(auctionId);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });  

            return Ok(result.Value);                             
        }

        [Authorize]
        [HttpGet("userBids")]
        public async Task<IActionResult> GetUserBidHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
            User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }

            var result = await _auctionService.GetUserBidHistoryAsync(userId);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("{id}/watch")]
        public async Task<IActionResult> AddToWatchlist(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                  User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }

            var result = await _auctionService.AddToWatchListAsync(userId, id);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            var watchList = result.Value!;
            return Ok(watchList);
        }

        [Authorize]
        [HttpDelete("{id}/watch")]
        public async Task<IActionResult> RemoveFromWatchlist(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
            User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }

            var result = await _auctionService.RemoveFromWatchListAsync(userId, id);
            if (!result.IsSuccess)
                return BadRequest(new {error = result.Error});  
            return Ok(result.Value);
        }

        [Authorize]
        [HttpGet("user/watchlist")]
        public async Task<IActionResult> GetUserWatchlist()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                              User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }

            var watchlists = await _auctionService.GetUserWatchListAsync(userId);

            if (!watchlists.IsSuccess)
                return NotFound(new { error = watchlists.Error });

            return Ok(watchlists.Value);
        }

        [HttpGet("{auctionId}/watchers")]
        public async Task<IActionResult> GetAuctionWatchers(int auctionId)
        {
            var watchlists = await _auctionService.GetAuctionWatchersAsync(auctionId);

            if (!watchlists.IsSuccess)
                return NotFound(new { error = watchlists.Error });
            return Ok(watchlists.Value);
        }

        [HttpGet("highest-bidder/{auctionId}")]
        public async Task<IActionResult> GetHighestBidderId(int auctionId)
        {
            var highestId = await _auctionService.GetHighestBidderIdAsync(auctionId);

            if (!highestId.IsSuccess)
                return NotFound(new { error = highestId.Error });
            return Ok(highestId.Value);
        }

        [HttpGet("{auctionId}/result")]
        public async Task<IActionResult> GetAuctionResult(int auctionId)
        {
            var result = await _auctionService.ProcessAuctionResultAsync(auctionId);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [Authorize] 
        [HttpGet("{id}/checkout")]
        public IActionResult VerifyCheckoutAccess(int id)
        {
            return Ok(new { success = true, message = "User authorized for auction checkout." });
        }
    }
}
