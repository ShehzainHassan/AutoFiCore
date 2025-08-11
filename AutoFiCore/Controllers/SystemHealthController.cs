using AutoFiCore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFiCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemHealthController : ControllerBase
    {
        private readonly ISystemHealthService _systemHealthService;

        public SystemHealthController(ISystemHealthService systemHealthService)
        {
            _systemHealthService = systemHealthService;
        }

        [HttpGet("performance-report")]
        public async Task<ActionResult<PerformanceReport>> GetPerformanceReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var report = await _systemHealthService.GetPerformanceReportAsync(utcStart, utcEnd);
            return Ok(report);
        }

        [HttpGet("error-report")]
        public async Task<ActionResult<ErrorReport>> GetErrorReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var report = await _systemHealthService.GetErrorReportAsync(utcStart, utcEnd);
            return Ok(report);
        }

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
