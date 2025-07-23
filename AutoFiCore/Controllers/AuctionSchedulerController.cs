using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auction")]
public class AuctionSchedulerController:ControllerBase
{
   private readonly IAuctionSchedulerService _service;
   public AuctionSchedulerController(IAuctionSchedulerService auctionSchedulerService)
   {
      _service = auctionSchedulerService;
   }
   [Authorize]
   [HttpPut("schedule/{id}")]
   public async Task<IActionResult> UpdateScheduledAuction([FromRoute(Name = "id")] int auctionId, [FromBody] CreateAuctionDTO dto)
   {
        var result = await _service.UpdateScheduledAuctionAsync(auctionId, dto);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
   }
}
