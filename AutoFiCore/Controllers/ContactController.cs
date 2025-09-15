using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoFiCore.Utilities;
using AutoFiCore.Data.Interfaces;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Provides endpoints for managing user contact information.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ContactController : SecureControllerBase
    {
        private readonly IContactInfoService _contactInfoService;
        private readonly ILogger<ContactController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactController"/> class.
        /// </summary>
        /// <param name="contactInfoService">Service for handling contact information operations.</param>
        public ContactController(IContactInfoService contactInfoService, ILogger<ContactController> logger)
        {
            _contactInfoService = contactInfoService;
            _logger = logger;
        }

        /// <summary>
        /// Adds new contact information for the authenticated user.
        /// </summary>
        /// <param name="contactInfo">The contact information to be added.</param>
        /// <returns>
        /// Returns the added <see cref="ContactInfo"/> object if successful.
        /// </returns>
        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddContactInfo([FromBody] ContactInfo contactInfo)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { message = "Invalid token or user context." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("AddContactInfo called. CorrelationId={CorrelationId}, UserId={UserId}, Phone={Phone}, Email={Email}",
                correlationId, userId, contactInfo.PhoneNumber, contactInfo.Email);

            var addedContact = await _contactInfoService.AddContactInfoAsync(contactInfo);
            return Ok(addedContact.Value);
        }
    }
}