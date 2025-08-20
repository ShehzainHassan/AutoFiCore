using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Handles scheduling and updating of auction events.
/// </summary>
[ApiController]
[Route("api/auction")]
public class AuctionSchedulerController:ControllerBase
{
   private readonly IAuctionSchedulerService _service;
    /// <summary>
    /// Initializes a new instance of the <see cref="AuctionSchedulerController"/> class.
    /// </summary>
    /// <param name="auctionSchedulerService">Service for managing auction scheduling.</param>
   public AuctionSchedulerController(IAuctionSchedulerService auctionSchedulerService)
   {
      _service = auctionSchedulerService;
   }

    /// <summary>
    /// Updates the scheduling details of an existing auction.
    /// </summary>
    /// <param name="auctionId">The ID of the auction to update.</param>
    /// <param name="dto">Auction creation details to apply.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> with updated auction data if successful; 
    /// otherwise returns <see cref="BadRequestObjectResult"/> with error details.
    /// </returns>

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
