using AutoFiCore.Data;
using AutoFiCore.Data.Interfaces;
using AutoFiCore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Provides endpoints for retrieving system health metrics such as performance, errors, and slow queries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SystemHealthController : ControllerBase
    {
        private readonly ISystemHealthService _systemHealthService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemHealthController"/> class.
        /// </summary>
        /// <param name="systemHealthService">Service for retrieving system health data.</param>
        public SystemHealthController(ISystemHealthService systemHealthService)
        {
            _systemHealthService = systemHealthService;
        }

        /// <summary>
        /// Retrieves a performance report for the specified date range.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("performance-report")]
        public async Task<IActionResult> GetPerformanceReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var result = await _systemHealthService.GetPerformanceReportAsync(utcStart, utcEnd);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves an error report for the specified date range.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("error-report")]
        public async Task<IActionResult> GetErrorReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var result = await _systemHealthService.GetErrorReportAsync(utcStart, utcEnd);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Identifies slow database queries executed within the specified date range.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("slow-queries")]
        public async Task<IActionResult> IdentifySlowQueries([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var result = await _systemHealthService.IdentifySlowQueriesAsync(utcStart, utcEnd);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(result.Value);
        }
    }
}
