using AutoFiCore.Data;
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
    private readonly IUnitOfWork _unitOfWork;

    public AutoBidController(IAutoBidService autoBidService, IUnitOfWork unitOfWork)
    {
        _autoBidService = autoBidService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAutoBid([FromBody] CreateAutoBidDTO dto)
    {
        var result = await _autoBidService.CreateAutoBidAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("update/auction/{auctionId}/user/{userId}")]
    [Authorize]
    public async Task<IActionResult> UpdateAutoBid(int auctionId, int userId, [FromBody] UpdateAutoBidDTO dto)
    {
        var result = await _autoBidService.UpdateAutoBidAsync(auctionId, userId, dto);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpDelete("{auctionId}/user/{userId}")]
    [Authorize]
    public async Task<IActionResult> CancelAutoBid(int userId, int auctionId)
    {
        var result = await _autoBidService.CancelAutoBidAsync(auctionId, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Auto-bid cancelled successfully" });
    }

    [HttpGet("auction/{auctionId}/user/{userId}")]
    public async Task<ActionResult<bool>> IsAutoBidSet(int auctionId, int userId)
    {
        var result = await _autoBidService.IsAutoBidSetAsync(auctionId, userId);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{auctionId}/user/{userId}")]
    public async Task<IActionResult> GetAutoBidWithStrategy(int userId, int auctionId)
    {
        var result = await _autoBidService.GetAutoBidWithStrategyAsync(userId, auctionId);

        if (!result.IsSuccess)
            return NotFound(new { message = result.Error });

        return Ok(result.Value);
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