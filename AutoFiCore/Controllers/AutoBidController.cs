using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateAutoBid([FromBody] CreateAutoBidDTO dto)
    {
        var result = await _autoBidService.CreateAutoBidAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

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

    [HttpGet("/api/auction/{auctionId}/autobids")]
    public async Task<ActionResult<AutoBidSummaryDto>> GetAuctionAutoBidSummary(int auctionId)
    {
        var summary = await _autoBidService.GetAuctionAutoBidSummaryAsync(auctionId);
        if (summary == null)
            return NotFound($"Auction with ID {auctionId} not found.");

        return Ok(summary);
    }
}
