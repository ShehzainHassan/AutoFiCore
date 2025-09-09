using AutoFiCore.Controllers;
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
public class AuctionSchedulerController:SecureControllerBase
{
   private readonly IAuctionSchedulerService _service;
    private readonly ILogger<AuctionSchedulerController> _logger;
    /// <summary>
    /// Initializes a new instance of the <see cref="AuctionSchedulerController"/> class.
    /// </summary>
    /// <param name="auctionSchedulerService">Service for managing auction scheduling.</param>
    public AuctionSchedulerController(IAuctionSchedulerService auctionSchedulerService, ILogger<AuctionSchedulerController> logger)
    {
        _service = auctionSchedulerService;
        _logger = logger;
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
        if (!IsUserContextValid(out var userId))
            return Unauthorized(new { message = "Invalid token or user context." });

        var correlationId = SetCorrelationIdHeader();
        _logger.LogInformation("UpdateScheduledAuction called. CorrelationId={CorrelationId}, UserId={UserId}, AuctionId={AuctionId}", correlationId, userId, auctionId);

        var result = await _service.UpdateScheduledAuctionAsync(auctionId, dto);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }
}
