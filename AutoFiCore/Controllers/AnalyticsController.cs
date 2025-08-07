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
        private readonly IMetricsCalculationService _metricsCalculationService;
        private readonly IDashboardService _dashboardService;
        private readonly IReportingService _reportService;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            IDashboardService dashboardService,
            IReportingService reportService,
            IMetricsCalculationService metricsCalculationService)
        {
            _analyticsService = analyticsService;
            _dashboardService = dashboardService;
            _reportService = reportService;
            _metricsCalculationService = metricsCalculationService;
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

        [HttpPost("update-auction-analytics")]
        public async Task<IActionResult> UpdateAuctionAnalytics([FromQuery] int auctionId)
        {
            if (auctionId <= 0)
                return BadRequest(new { error = "Invalid auction ID." });

            await _metricsCalculationService.UpdateAuctionAnalyticsAsync(auctionId);
            return Ok(new { message = $"Auction analytics updated for auction {auctionId}" });
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

        [HttpPost("track-bid")]
        public async Task<IActionResult> TrackBidEvent([FromBody] BidTrackingDTO bid)
        {
            await _analyticsService.TrackBidEventAsync(bid.AuctionId, bid.UserId, bid.Amount);
            return Ok(new { message = "Bid tracked" });
        }

        [HttpPost("track-payment")]
        public async Task<IActionResult> TrackPayment([FromBody] BidTrackingDTO bid)
        {
            var alreadyCompleted = await _analyticsService.IsAuctionPaymentCompleted(bid.AuctionId);
            if (alreadyCompleted)
            {
                return BadRequest(new { message = "Payment has already been tracked for this auction." });
            }

            await _analyticsService.TrackPaymentCompleted(bid.AuctionId, bid.UserId, bid.Amount);
            return Ok(new { message = "Payment tracked" });
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
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var result = await _reportService.GetAuctionPerformanceReportAsync(utcStart, utcEnd);
            return Ok(result);
        }

        [HttpGet("auctions-report")]
        public async Task<IActionResult> GetAuctionTableAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string? category)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var result = await _reportService.GetAuctionAnalyticsAsync(utcStart, utcEnd, category);
            return Ok(result);
        }

        [HttpGet("user-report")]
        public async Task<IActionResult> GetUserTableAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var result = await _reportService.GetUserAnalyticsAsync(utcStart, utcEnd);
            return Ok(result);
        }

        [HttpGet("revenue-report")]
        public async Task<IActionResult> GetRevenueTableAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var result = await _reportService.GetRevenueTableAnalyticsAsync(utcStart, utcEnd);
            return Ok(result);
        }
        [HttpGet("payment-status")]
        public async Task<IActionResult> IsPaymentCompleted([FromQuery] int auctionId)
        {
            bool isCompleted = await _analyticsService.IsAuctionPaymentCompleted(auctionId);
            return Ok(new
            {
                auctionId,
                paymentCompleted = isCompleted
            });
        }
        [HttpGet("users")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserActivityReport>> GetUserAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var result = await _reportService.GetUserActivityReportAsync(utcStart, utcEnd);
            return Ok(result);
        }

        [HttpGet("revenue")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<RevenueReport>> GetRevenueAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var result = await _reportService.GetRevenueReportAsync(utcStart, utcEnd);
            return Ok(result);
        }

        [HttpPost("event")]
        public async Task<IActionResult> TrackEvent([FromBody] TrackEventRequest request)
        {
            await _analyticsService.TrackEventAsync(request.Type, request.UserId, request.AuctionId, request.Properties ?? new(), "Web");
            return Ok();
        }


        [HttpGet("revenue-summary")]
        public async Task<IActionResult> GetRevenueSummary([FromQuery] SummaryPeriod period)
        {
            var summary = await _reportService.GetRevenueSummaryAsync(period);
            return Ok(summary);
        }

        [HttpGet("user-summary")]
        public async Task<IActionResult> GetUserSummary([FromQuery] SummaryPeriod period)
        {
            var summary = await _reportService.GetUserRegistrationSummaryAsync(period);
            return Ok(summary);
        }

        [HttpGet("export")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportReport([FromQuery] string reportType, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "csv")
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var fileResult = await _reportService.ExportReportAsync(reportType, utcStart, utcEnd, format);
            return File(fileResult.Content, fileResult.ContentType, fileResult.FileName);
        }
    }
}
