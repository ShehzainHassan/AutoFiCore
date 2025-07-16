using AutoFiCore.Dto;
using AutoFiCore.Mappers;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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
        [HttpPost("create-auction")]
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

        [HttpGet("userBids/{id}")]
        public async Task<IActionResult> GetUserBidHistory([FromRoute(Name = "id")] int userId)
        {
            var result = await _auctionService.GetUserBidHistoryAsync(userId);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("{id}/watch")]
        public async Task<IActionResult> AddToWatchlist(int id, [FromQuery] int userId)
        {
            var result = await _auctionService.AddToWatchListAsync(userId, id);
            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            var watchList = result.Value!;
            return Ok(watchList);
        }

        [Authorize]
        [HttpDelete("{id}/watch")]
        public async Task<IActionResult> RemoveFromWatchlist(int id, [FromQuery] int userId)
        {
            var result = await _auctionService.RemoveFromWatchListAsync(userId, id);
            if (!result.IsSuccess)
                return BadRequest(new {error = result.Error});  
            return Ok(result.Value);
        }

        [HttpGet("user/{userId}/watchlist")]
        public async Task<IActionResult> GetUserWatchlist(int userId)
        {
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

    }
}
