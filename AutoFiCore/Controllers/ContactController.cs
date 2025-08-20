using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoFiCore.Utilities;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Provides endpoints for managing user contact information.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IContactInfoService _contactInfoService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactController"/> class.
        /// </summary>
        /// <param name="contactInfoService">Service for handling contact information operations.</param>
        public ContactController(IContactInfoService contactInfoService)
        {
            _contactInfoService = contactInfoService;
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
        public async Task<ActionResult<ContactInfo>> AddContactInfo([FromBody] ContactInfo contactInfo)
        {
            var addedContact = await _contactInfoService.AddContactInfoAsync(contactInfo);
            return Ok(addedContact);
        }
    }
}