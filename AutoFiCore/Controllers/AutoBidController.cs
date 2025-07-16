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
        var userId = GetUserId();
        var result = await _autoBidService.CreateAutoBidAsync(dto, userId);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    private int GetUserId()
    {
        var idClaim = User.FindFirst("id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim?.Value, out var id) ? id : -1;
    }
}
