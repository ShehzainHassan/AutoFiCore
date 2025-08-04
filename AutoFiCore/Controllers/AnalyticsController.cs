using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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
        [Authorize]
        [HttpPost("auction-view")]
        public async Task<IActionResult> TrackAuctionView([FromQuery] int auctionId, [FromQuery] string source = "Web")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
            User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }

            await _analyticsService.TrackAuctionViewAsync(auctionId, userId, source);
            return Ok(new { message = "Auction view tracked" });
        }

        [Authorize]
        [HttpPost("track-bid")]
        public async Task<IActionResult> TrackBidEvent([FromBody] BidTrackingDTO bid)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                      User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });
            }
            await _analyticsService.TrackBidEventAsync(bid.AuctionId, userId, bid.Amount);
            return Ok(new { message = "Bid tracked" });
        }

        [HttpPost("auction-completion")]
        public async Task<IActionResult> TrackAuctionCompletion([FromBody] AuctionCompletionDTO completion)
        {
            await _analyticsService.TrackAuctionCompletionAsync(completion.AuctionId, completion.IsSuccessful, completion.FinalPrice);
            return Ok(new { message = "Auction completion tracked" });
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
