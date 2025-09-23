using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Provides endpoints for tracking and retrieving auction-related analytics, metrics, and reports.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : SecureControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IMetricsCalculationService _metricsCalculationService;
        private readonly IDashboardService _dashboardService;
        private readonly IReportingService _reportService;
        private readonly ISystemHealthService _systemHealthService;
        private readonly ILogger<AnalyticsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsController"/> class.
        /// </summary>
        /// <param name="analyticsService">Service for tracking analytics events.</param>
        /// <param name="dashboardService">Service for generating executive dashboards.</param>
        /// <param name="reportService">Service for generating detailed reports.</param>
        /// <param name="metricsCalculationService">Service for calculating metrics.</param>
        /// <param name="systemHealthService">Service for monitoring system health.</param>

        public AnalyticsController(IAnalyticsService analyticsService, IDashboardService dashboardService, IReportingService reportService, IMetricsCalculationService metricsCalculationService, ISystemHealthService systemHealthService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _dashboardService = dashboardService;
            _reportService = reportService;
            _metricsCalculationService = metricsCalculationService;
            _systemHealthService = systemHealthService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves executive dashboard metrics for the specified date range.
        /// </summary>
        /// <param name="startDate">Start date (UTC).</param>
        /// <param name="endDate">End date (UTC).</param>
        /// <returns>Executive dashboard data.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("dashboard")]
        //[Authorize(Roles = "Admin", "Manager")]
        public async Task<IActionResult> GetDashboard([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            var dashboard = await _dashboardService.GetExecutiveDashboardAsync(utcStart, utcEnd);
            return Ok(dashboard.Value);
        }

        /// <summary>
        /// Updates analytics for a specific auction.
        /// </summary>
        /// <param name="auctionId">The ID of the auction to update.</param>
        /// <returns>Status message.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpPost("update-auction-analytics")]
        public async Task<IActionResult> UpdateAuctionAnalytics([FromQuery] int auctionId)
        {
            if (auctionId <= 0)
                return BadRequest(new { error = "Invalid auction ID." });

            await _metricsCalculationService.UpdateAuctionAnalyticsAsync(auctionId);
            return Ok(new { message = $"Auction analytics updated for auction {auctionId}" });
        }

        /// <summary>
        /// Tracks a user's view of an auction.
        /// </summary>
        /// <param name="auctionId">The ID of the auction viewed.</param>
        /// <param name="source">The source of the view (e.g., Web, Mobile).</param>
        /// <returns>Status message.</returns>
        [Authorize]
        [DisableRateLimiting]
        [HttpPost("auction-view")]
        public async Task<IActionResult> TrackAuctionView([FromQuery] int auctionId, [FromQuery] string source = "Web")
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Unauthorized: Missing or invalid user ID." });

            var correlationId = GetCorrelationId();
            _logger.LogInformation("TrackAuctionView called. CorrelationId={CorrelationId}, UserId={UserId}, AuctionId={AuctionId}, Source={Source}",
                correlationId, userId, auctionId, source);

            await _analyticsService.TrackAuctionViewAsync(auctionId, userId, source);
            return Ok(new { message = "Auction view tracked" });
        }

        /// <summary>
        /// Tracks a bid event for analytics purposes.
        /// </summary>
        /// <param name="bid">Bid tracking data.</param>
        /// <returns>Status message.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpPost("track-bid")]
        public async Task<IActionResult> TrackBidEvent([FromBody] BidTrackingDTO bid)
        {
            await _analyticsService.TrackBidEventAsync(bid.AuctionId, bid.UserId, bid.Amount);
            return Ok(new { message = "Bid tracked" });
        }

        /// <summary>
        /// Tracks payment completion for an auction.
        /// </summary>
        /// <param name="bid">Bid tracking data.</param>
        /// <returns>Status message or error if already completed.</returns>
        [DisableRateLimiting]
        [HttpPost("track-payment")]
        public async Task<IActionResult> TrackPayment([FromBody] BidTrackingDTO bid)
        {
            var paymentResult = await _analyticsService.IsAuctionPaymentCompletedAsync(bid.AuctionId);

            if (!paymentResult.IsSuccess)
                return StatusCode(500, new { message = paymentResult.Error }); 

            if (paymentResult.Value)
                return BadRequest(new { message = "Payment has already been tracked for this auction." });

            var trackResult = await _analyticsService.TrackPaymentCompletedAsync(bid.AuctionId, bid.UserId, bid.Amount);

            if (!trackResult.IsSuccess)
                return StatusCode(500, new { message = trackResult.Error });

            return Ok(new { message = "Payment tracked" });
        }


        /// <summary>
        /// Tracks the completion status of an auction.
        /// </summary>
        /// <param name="completion">Auction completion data.</param>
        /// <returns>Status message.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpPost("auction-completion")]
        public async Task<IActionResult> TrackAuctionCompletion([FromBody] AuctionCompletionDTO completion)
        {
            await _analyticsService.TrackAuctionCompletionAsync(completion.AuctionId, completion.IsSuccessful, completion.FinalPrice);
            return Ok(new { message = "Auction completion tracked" });
        }

        /// <summary>
        /// Retrieves auction performance metrics for the specified date range.
        /// </summary>
        /// <param name="startDate">Start date (UTC).</param>
        /// <param name="endDate">End date (UTC).</param>
        /// <returns>Auction performance report.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("auctions")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAuctionAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var result = await _dashboardService.GetAuctionDashboardAsync(utcStart, utcEnd);
            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves tabular auction analytics for the specified date range and optional category.
        /// </summary>
        /// <param name="startDate">Start date (UTC).</param>
        /// <param name="endDate">End date (UTC).</param>
        /// <param name="category">Optional category filter.</param>
        /// <returns>Tabular auction analytics.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("auctions-report")]
        public async Task<IActionResult> GetAuctionTableAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string? category)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var result = await _reportService.GetAuctionAnalyticsAsync(utcStart, utcEnd, category);
            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves user engagement analytics for the specified date range.
        /// </summary>
        /// <param name="startDate">Start date (UTC).</param>
        /// <param name="endDate">End date (UTC).</param>
        /// <returns>User analytics report.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("user-report")]
        public async Task<IActionResult> GetUserTableAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var result = await _reportService.GetUserAnalyticsAsync(utcStart, utcEnd);
            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves revenue analytics for the specified date range.
        /// </summary>
        /// <param name="startDate">Start date (UTC).</param>
        /// <param name="endDate">End date (UTC).</param>
        /// <returns>Revenue analytics report.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("revenue-report")]
        public async Task<IActionResult> GetRevenueTableAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var result = await _reportService.GetRevenueTableAnalyticsAsync(utcStart, utcEnd);
            return Ok(result.Value);
        }

        /// <summary>
        /// Checks whether payment has been completed for a specific auction.
        /// </summary>
        /// <param name="auctionId">The ID of the auction.</param>
        /// <returns>Payment status.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("payment-status")]
        public async Task<IActionResult> IsPaymentCompleted([FromQuery] int auctionId)
        {
            var result = await _analyticsService.IsAuctionPaymentCompletedAsync(auctionId);

            if (!result.IsSuccess)
            {
                return StatusCode(500, new
                {
                    auctionId,
                    error = result.Error
                });
            }

            return Ok(new
            {
                auctionId,
                paymentCompleted = result.Value
            });
        }


        /// <summary>
        /// Retrieves user activity analytics for the specified date range.
        /// </summary>
        /// <param name="startDate">Start date (UTC).</param>
        /// <param name="endDate">End date (UTC).</param>
        /// <returns>User activity report.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("users")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var result = await _dashboardService.GetUserDashboardAsync(utcStart, utcEnd);
            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves revenue analytics for the specified date range.
        /// </summary>
        /// <param name="startDate">Start date (UTC).</param>
        /// <param name="endDate">End date (UTC).</param>
        /// <returns>Revenue report.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("revenue")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRevenueAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var result = await _reportService.GetRevenueReportAsync(utcStart, utcEnd);
            return Ok(result.Value);
        }

        /// <summary>
        /// Tracks a custom event related to user or auction activity.
        /// </summary>
        /// <param name="request">Event tracking request payload.</param>
        /// <returns>Status message.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpPost("event")]
        public async Task<IActionResult> TrackEvent([FromBody] TrackEventRequest request)
        {
            await _analyticsService.TrackEventAsync(request.Type, request.UserId, request.AuctionId, request.Properties ?? new(), "Web");
            return Ok();
        }

        /// <summary>
        /// Retrieves a summarized view of revenue analytics.
        /// </summary>
        /// <param name="startDate">Start date (UTC).</param>
        /// <param name="endDate">End date (UTC).</param>
        /// <param name="type">Type of summary (e.g., Revenue or Users).</param>
        /// <returns>Revenue summary data.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("revenue-summary")]
        public async Task<IActionResult> GetRevenueSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string type)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var summary = await _reportService.GetSummaryAsync("Revenue", utcStart, utcEnd);
            return Ok(summary.Value);
        }

        /// <summary>
        /// Retrieves a summarized view of user analytics.
        /// </summary>
        /// <param name="startDate">Start date (UTC).</param>
        /// <param name="endDate">End date (UTC).</param>
        /// <param name="type">Type of summary (e.g., Revenue or Users).</param>
        /// <returns>User summary data.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("user-summary")]
        public async Task<IActionResult> GetUserSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string type)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var summary = await _reportService.GetSummaryAsync("Users", utcStart, utcEnd);
            return Ok(summary.Value);
        }

        /// <summary>
        /// Exports a report in the specified format for the given date range.
        /// </summary>
        /// <param name="reportType">Type of report to export.</param>
        /// <param name="startDate">Start date (UTC).</param>
        /// <param name="endDate">End date (UTC).</param>
        /// <param name="format">Export format (e.g., CSV, PDF).</param>
        /// <returns>File download response.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("export")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportReport([FromQuery] ReportType reportType, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "CSV")
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);

            var result = await _reportService.ExportReportAsync(reportType, utcStart, utcEnd, format);

            if (!result.IsSuccess)
            {
                return BadRequest(new { error = result.Error });
            }

            var fileResult = result.Value!;

            return File(fileResult.Content, fileResult.ContentType, fileResult.FileName);
        }


        /// <summary>
        /// Retrieves a paginated list of recently downloaded reports.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>List of recent downloads.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("recent-downloads")]
        public async Task<IActionResult> GetRecentDownloads([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _reportService.GetRecentDownloadsAsync(page, pageSize);
            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves system health dashboard metrics for the specified date range.
        /// </summary>
        /// <param name="startDate">Start date (UTC).</param>
        /// <param name="endDate">End date (UTC).</param>
        /// <returns>System health dashboard.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("system")]
        public async Task<IActionResult> GetSystemHealthDashboard([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Utc);
            var dashboard = await _systemHealthService.GetSystemHealthDashboardAsync(utcStart, utcEnd);
            return Ok(dashboard.Value);
        }

        /// <summary>
        /// Retrieves a paginated list of system error logs within the specified date range.
        /// </summary>
        /// <param name="startDate">Filter logs created after or equal to this date (required).</param>
        /// <param name="endDate">Filter logs created before or equal to this date (required).</param>
        /// <param name="page">Page number (optional).</param>
        /// <param name="pageSize">Number of logs per page (optional).</param>
        /// <returns>Error log entries.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("error-logs")]
        public async Task<IActionResult> GetErrorLogs(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var utcStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var result = await _systemHealthService.GetErrorLogsPagedAsync(page, pageSize, utcStart, utcEnd);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }



        /// <summary>
        /// Retrieves response time data points for the specified date range.
        /// </summary>
        /// <param name="startDate">Start date (UTC).</param>
        /// <param name="endDate">End date (UTC).</param>
        /// <returns>Response time metrics.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("response-times")]
        public async Task<IActionResult> GetResponseTimePoints([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate == default || endDate == default)
                return BadRequest("Start date and end date are required.");

            if (endDate < startDate)
                return BadRequest("End date must be greater than or equal to start date.");

            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var result = await _systemHealthService.GetResponseTimePointsAsync(startDate, endDate);

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the timestamp of the oldest API log entry.
        /// </summary>
        /// <returns>Oldest API log timestamp.</returns>
        [AllowAnonymous]
        [DisableRateLimiting]
        [HttpGet("oldest-api-log")]
        public async Task<IActionResult> GetOldestApiLog()
        {
            var result = await _systemHealthService.GetOldestApiLogTimestampAsync();
            return Ok(result.Value);
        }
    }
}
