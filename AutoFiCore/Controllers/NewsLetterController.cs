using AutoFiCore.Data.Interfaces;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Provides endpoints for newsletter subscription management.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class NewsLetterController : ControllerBase
    {
        private readonly INewsLetterService _newsLetterService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsLetterController"/> class.
        /// </summary>
        /// <param name="newsLetterService">Service for handling newsletter subscriptions.</param>
        public NewsLetterController(INewsLetterService newsLetterService)
        {
            _newsLetterService = newsLetterService;
        }

        /// <summary>
        /// Subscribes a user to the newsletter using their email address.
        /// </summary>
        /// <param name="newsletter">The newsletter subscription request containing the user's email.</param>
        /// <returns>
        /// Returns the subscribed <see cref="Newsletter"/> object if successful;
        /// otherwise returns a <see cref="BadRequestObjectResult"/> or <see cref="ConflictObjectResult"/> with error details.
        /// </returns>
        [AllowAnonymous]
        [HttpPost("subscribe-email")]
        public async Task<IActionResult> AddEmailToSubscribe(Newsletter newsletter)
        {
            var result = await _newsLetterService.SubscribeToNewsLetterAsync(newsletter);

            if (result.IsSuccess)
                return Ok(result.Value);

            if (!string.IsNullOrEmpty(result.Error) && result.Error.Contains("already subscribed", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { message = result.Error });

            if (result.Errors != null && result.Errors.Any())
                return BadRequest(new { errors = result.Errors });

            return BadRequest(new { message = result.Error });
        }

    }
}