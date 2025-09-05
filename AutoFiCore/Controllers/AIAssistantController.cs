using AutoFiCore.Dto;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;
using System.Text;


namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Controller for interfacing with the AI Assistant service.
    /// Provides endpoints for querying AI, retrieving user context, and generating suggestions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AIAssistantController : ControllerBase
    {
        private readonly IAIAssistantService _aiService;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<AIAssistantController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AIAssistantController"/> class.
        /// </summary>
        /// <param name="aiService">Service for handling AI assistant operations.</param>
        /// <param name="logger">Logger for tracking AI assistant activity.</param>

        public AIAssistantController(IAIAssistantService aiService, IUserContextService userContextService, ILogger<AIAssistantController> logger)
        {
            _aiService = aiService;
            _userContextService = userContextService;
            _logger = logger;
        }

        /// <summary>
        /// Sends a query payload to the FastAPI AI service and returns the response.
        /// </summary>
        /// <param name="payload">The query payload to send to the AI service.</param>
        /// <returns>AI-generated response based on the input payload.</returns>
        [HttpPost("query")]
        [Authorize]
        public async Task<ActionResult<AIResponseModel>> QueryAI([FromBody] EnrichedAIQuery payload)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { error = "Missing user context." });

            if (payload.UserId.ToString() != userIdClaim)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "User ID mismatch between token and payload." });

            var correlationId = Guid.NewGuid().ToString();
            HttpContext.Response.Headers["X-Correlation-ID"] = correlationId;

            _logger.LogInformation("AI Query initiated. CorrelationId={CorrelationId}, UserId={UserId}, Question={Question}",
                correlationId, payload.UserId, payload.Query.Question);

            var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _aiService.QueryFastApiAsync(payload, correlationId, jwtToken);

            _logger.LogInformation("AI Query completed. CorrelationId={CorrelationId}, Answer={Answer}", correlationId, result.Answer);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the user's auction and vehicle history to provide context for AI suggestions.
        /// </summary>
        /// <returns>User-specific auction, watchlist, and vehicle data.</returns>
        [HttpGet("user-context")]
        [Authorize]
        public async Task<IActionResult> GetUserContext()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var context = await _userContextService.GetUserContextAsync(userId);
            return Ok(context);
        }

        /// <summary>
        /// Retrieves contextual AI-generated suggestions based on the user's auction and vehicle history.
        /// </summary>
        /// <returns>List of personalized suggestions from the AI assistant.</returns>
        [HttpGet("contextual-suggestions/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetContextualSuggestions(int userId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { error = "Missing user context." });

            if (userId.ToString() != userIdClaim)
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "User ID mismatch" });

            var correlationId = Guid.NewGuid().ToString();
            HttpContext.Response.Headers["X-Correlation-ID"] = correlationId;

            _logger.LogInformation("Contextual suggestion request received. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var suggestions = await _aiService.GetSuggestionsAsync(userId, correlationId, jwtToken);

            return Ok(suggestions);
        }

        [HttpGet("chats")]
        [Authorize]
        public async Task<IActionResult> GetChatTitles()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "Missing user context." });

            var titles = await _aiService.GetChatTitlesAsync(int.Parse(userId));
            return Ok(titles);
        }

        [HttpGet("chats/{sessionId}")]
        [Authorize]
        public async Task<IActionResult> GetChat(string sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "Missing user context." });

            var chat = await _aiService.GetFullChatAsync(int.Parse(userId), sessionId);
            if (chat == null) return NotFound(new { error = "Chat not found." });

            return Ok(chat);
        }

        [Authorize]
        [HttpDelete("chats/{sessionId}")]
        public async Task<IActionResult> DeleteSession(string sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "Missing user context." });

            await _aiService.DeleteSessionAsync(sessionId, int.Parse(userId));
            return NoContent();
        }

        [Authorize]
        [HttpDelete("chats")]
        public async Task<IActionResult> DeleteAllSessions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "Missing user context." });

            await _aiService.DeleteAllSessionsAsync(int.Parse(userId));
            return NoContent();
        }

        [HttpPut("chats/{sessionId}/title")]
        [Authorize]
        public async Task<IActionResult> UpdateSessionTitle(string sessionId, [FromBody] UpdateChatTitle request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "Missing user context." });

            if (string.IsNullOrWhiteSpace(request.NewTitle))
                return BadRequest(new { error = "Title cannot be empty." });

            try
            {
                await _aiService.UpdateSessionTitleAsync(sessionId, int.Parse(userId), request.NewTitle);
                return Ok("Session updated successfully");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update title for session {sessionId}.");
                return StatusCode(500, new { error = "Internal server error." });
            }
        }

        [HttpPost("feedback")]
        [Authorize]
        public async Task<IActionResult> SubmitFeedback([FromBody] AIQueryFeedbackDto feedbackDto)
        {
            var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(jwtToken))
                return Unauthorized(new { error = "Missing JWT token." });

            var correlationId = Guid.NewGuid().ToString();
            HttpContext.Response.Headers["X-Correlation-ID"] = correlationId;

            try
            {
                var result = await _aiService.SubmitFeedbackAsync(feedbackDto, correlationId, jwtToken);
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = ex.Message, correlationId });
            }
        }

        /// <summary>
        /// Retrieves the top popular queries from the FastAPI service.
        /// </summary>
        [HttpGet("popular-queries")]
        [Authorize]
        public async Task<ActionResult<List<PopularQueryDto>>> GetPopularQueries([FromQuery] int limit = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "Missing user context." });

            if (limit < 1 || limit > 50)
                return BadRequest(new { error = "Limit must be between 1 and 50." });

            var correlationId = Guid.NewGuid().ToString();
            HttpContext.Response.Headers["X-Correlation-ID"] = correlationId;

            _logger.LogInformation("Popular queries request. CorrelationId={CorrelationId}, UserId={UserId}, Limit={Limit}",
                correlationId, userId, limit);

            var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            try
            {
                var queries = await _aiService.GetPopularQueriesAsync(limit, correlationId, jwtToken);
                return Ok(queries);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch popular queries. CorrelationId={CorrelationId}", correlationId);
                return StatusCode(500, new { error = ex.Message, correlationId });
            }
        }
    
    }
}