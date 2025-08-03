using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFiCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IDashboardService _dashboardService;
        private readonly IReportingService _reportService;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            IDashboardService dashboardService,
            IReportingService reportService)
        {
            _analyticsService = analyticsService;
            _dashboardService = dashboardService;
            _reportService = reportService;
        }

        [HttpGet("dashboard")]
        //[Authorize(Roles = "Admin", "Manager")]
        public async Task<ActionResult<ExecutiveDashboard>> GetDashboard([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var dashboard = await _dashboardService.GetExecutiveDashboardAsync(utcStart, utcEnd);
            return Ok(dashboard);
        }

        [HttpGet("auctions")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<AuctionPerformanceReport>> GetAuctionAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var result = await _reportService.GetAuctionPerformanceReportAsync(utcStart, utcEnd);
            return Ok(result);
        }

        [HttpGet("users")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserActivityReport>> GetUserAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var result = await _reportService.GetUserActivityReportAsync(utcStart, utcEnd);
            return Ok(result);
        }

        [HttpGet("revenue")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<RevenueReport>> GetRevenueAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var result = await _reportService.GetRevenueReportAsync(utcStart, utcEnd);
            return Ok(result);
        }

        [HttpPost("event")]
        public async Task<IActionResult> TrackEvent([FromBody] TrackEventRequest request)
        {
            await _analyticsService.TrackEventAsync(request.Type, request.UserId, request.AuctionId, request.Properties ?? new(), "Web");
            return Ok();
        }

        [HttpGet("export")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportReport([FromQuery] string reportType, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "csv")
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var fileResult = await _reportService.ExportReportAsync(reportType, utcStart, utcEnd, format);
            return File(fileResult.Content, fileResult.ContentType, fileResult.FileName);
        }
    }
}
