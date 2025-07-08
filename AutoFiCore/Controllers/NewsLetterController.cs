using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFiCore.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class NewsLetterController:ControllerBase
    {
        private readonly INewsLetterService _newsLetterService;

        public NewsLetterController(INewsLetterService newsLetterService)
        {
            _newsLetterService = newsLetterService;
        }

        [HttpPost("subscribe-email")]

        public async Task<ActionResult<Newsletter>> AddEmailToSubscribe([FromBody] Newsletter newsletter)
        {
            var result = await _newsLetterService.SubscribeToNewsLetterAsync(newsletter);
            if (!result.IsSuccess)
            {
                if (result.Errors.Any())
                    return BadRequest(new { errors = result.Errors });
                return Conflict(new { message = result.Error });
            }
            return Ok(result.Value);
        }
    }
}
