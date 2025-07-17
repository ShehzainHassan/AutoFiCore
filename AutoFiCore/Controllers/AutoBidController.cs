using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AutoBidController : ControllerBase
{
    private readonly IAutoBidService _autoBidService;

    public AutoBidController(IAutoBidService autoBidService)
    {
        _autoBidService = autoBidService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAutoBid([FromBody] CreateAutoBidDTO dto)
    {
        var userId = 24;
        var result = await _autoBidService.CreateAutoBidAsync(dto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateAutoBid(int id, [FromBody] UpdateAutoBidDTO dto)
    {
        var userId = 24; 
        var result = await _autoBidService.UpdateAutoBidAsync(id, dto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> CancelAutoBid(int id)
    {
        var userId = 24;
        var result = await _autoBidService.CancelAutoBidAsync(id, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new {message = "Auto-bid cancelled successfully"});
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<AutoBidDTO>>> GetActiveAutoBidsForUser(int userId)
    {
        var result = await _autoBidService.GetActiveAutoBidsForUserAsync(userId);

        if (result == null || !result.Any())
            return NotFound("No active auto-bids found for this user.");

        return Ok(result);
    }

    [HttpGet("/api/auction/{auctionId}/autobids")]
    public async Task<ActionResult<AutoBidSummaryDto>> GetAuctionAutoBidSummary(int auctionId)
    {
        var summary = await _autoBidService.GetAuctionAutoBidSummaryAsync(auctionId);
        if (summary == null)
            return NotFound($"Auction with ID {auctionId} not found.");

        return Ok(summary);
    }


}
