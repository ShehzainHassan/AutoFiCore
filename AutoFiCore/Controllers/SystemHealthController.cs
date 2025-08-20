using AutoFiCore.Data;
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
        /// <param name="startDate">Start date of the report range.</param>
        /// <param name="endDate">End date of the report range.</param>
        /// <returns>A <see cref="PerformanceReport"/> containing system performance metrics.</returns>
        [HttpGet("performance-report")]
        public async Task<ActionResult<PerformanceReport>> GetPerformanceReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var report = await _systemHealthService.GetPerformanceReportAsync(utcStart, utcEnd);
            return Ok(report);
        }

        /// <summary>
        /// Retrieves an error report for the specified date range.
        /// </summary>
        /// <param name="startDate">Start date of the report range.</param>
        /// <param name="endDate">End date of the report range.</param>
        /// <returns>An <see cref="ErrorReport"/> containing system error statistics.</returns>
        [HttpGet("error-report")]
        public async Task<ActionResult<ErrorReport>> GetErrorReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var report = await _systemHealthService.GetErrorReportAsync(utcStart, utcEnd);
            return Ok(report);
        }

        /// <summary>
        /// Identifies slow database queries executed within the specified date range.
        /// </summary>
        /// <param name="startDate">Start date of the query analysis range.</param>
        /// <param name="endDate">End date of the query analysis range.</param>
        /// <returns>A list of <see cref="SlowQueryEntry"/> objects representing slow queries.</returns>
        [HttpGet("slow-queries")]
        public async Task<ActionResult<List<SlowQueryEntry>>> IdentifySlowQueries([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var queries = await _systemHealthService.IdentifySlowQueriesAsync(utcStart, utcEnd);
            return Ok(queries);
        }
    }
}