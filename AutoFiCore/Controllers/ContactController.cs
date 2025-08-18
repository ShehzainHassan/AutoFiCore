using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoFiCore.Utilities;

namespace AutoFiCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IContactInfoService _contactInfoService;

        public ContactController(IContactInfoService contactInfoService)
        {
            _contactInfoService = contactInfoService;
        }
        [Authorize]

        [HttpPost("add")]

        public async Task<ActionResult<ContactInfo>> AddContactInfo([FromBody] ContactInfo contactInfo)
        {
            var addedContact = await _contactInfoService.AddContactInfoAsync(contactInfo);
            return Ok(addedContact);
        }

    }
}
