using AutoFiCore.Dto;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text;
using System.Text.Json;


namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Controller for interfacing with the AI Assistant service.
    /// Provides endpoints for querying AI, retrieving user context, and generating suggestions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AIAssistantController : SecureControllerBase
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
        [Authorize]
        [HttpPost("query")]
        public async Task<ActionResult<AIResponseModel>> QueryAI([FromBody] EnrichedAIQuery payload)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            if (payload.UserId != userId)
                return Forbid("User ID mismatch between token and payload.");

            var correlationId = SetCorrelationIdHeader();
            _logger.LogInformation("AI Query initiated. CorrelationId={CorrelationId}, UserId={UserId}, Question={Question}",
                correlationId, userId, payload.Query.Question);

            var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _aiService.QueryFastApiAsync(payload, correlationId, jwtToken);

            _logger.LogInformation("AI Query completed. CorrelationId={CorrelationId}, Answer={Answer}", correlationId, result.Answer);
            return Ok(result);
        }


        /// <summary>
        /// Retrieves the user's auction and vehicle history to provide context for AI suggestions.
        /// </summary>
        /// <returns>User-specific auction, watchlist, and vehicle data.</returns>
        [Authorize]
        [HttpGet("user-context")]
        public async Task<IActionResult> GetUserContext()
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = SetCorrelationIdHeader();
            _logger.LogInformation("GetUserContext called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var context = await _userContextService.GetUserContextAsync(userId);
            return Ok(context);
        }


        /// <summary>
        /// Retrieves contextual AI-generated suggestions based on the user's auction and vehicle history.
        /// </summary>
        /// <returns>List of personalized suggestions from the AI assistant.</returns>
        [Authorize]
        [HttpGet("contextual-suggestions/{userId}")]
        public async Task<IActionResult> GetContextualSuggestions(int userId)
        {
            if (!IsUserContextValid(out var tokenUserId))
                return Unauthorized(new { error = "Missing user context." });

            if (userId != tokenUserId)
                return Forbid("User ID mismatch between token and route.");

            var correlationId = SetCorrelationIdHeader();
            _logger.LogInformation("Contextual suggestion request. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var suggestions = await _aiService.GetSuggestionsAsync(userId, correlationId, jwtToken);

            return Ok(suggestions);
        }


        [Authorize]
        [HttpGet("chats")]
        public async Task<IActionResult> GetChatTitles()
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = SetCorrelationIdHeader();
            _logger.LogInformation("GetChatTitles called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var titles = await _aiService.GetChatTitlesAsync(userId);
            return Ok(titles);
        }


        [Authorize]
        [HttpGet("chats/{sessionId}")]
        public async Task<IActionResult> GetChat(string sessionId)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = SetCorrelationIdHeader();
            _logger.LogInformation("GetChat called. CorrelationId={CorrelationId}, UserId={UserId}, SessionId={SessionId}", correlationId, userId, sessionId);

            var chat = await _aiService.GetFullChatAsync(userId, sessionId);
            return chat != null ? Ok(chat) : NotFound(new { error = "Chat not found." });
        }


        [Authorize]
        [HttpDelete("chats/{sessionId}")]
        public async Task<IActionResult> DeleteSession(string sessionId)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = SetCorrelationIdHeader();
            _logger.LogInformation("DeleteSession called. CorrelationId={CorrelationId}, UserId={UserId}, SessionId={SessionId}", correlationId, userId, sessionId);

            await _aiService.DeleteSessionAsync(sessionId, userId);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("chats")]
        public async Task<IActionResult> DeleteAllSessions()
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = SetCorrelationIdHeader();
            _logger.LogInformation("DeleteAllSessions called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            await _aiService.DeleteAllSessionsAsync(userId);
            return NoContent();
        }

        [Authorize]
        [HttpPut("chats/{sessionId}/title")]
        public async Task<IActionResult> UpdateSessionTitle(string sessionId, [FromBody] UpdateChatTitle request)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            if (string.IsNullOrWhiteSpace(request.NewTitle))
                return BadRequest(new { error = "Title cannot be empty." });

            var correlationId = SetCorrelationIdHeader();
            _logger.LogInformation("UpdateSessionTitle called. CorrelationId={CorrelationId}, UserId={UserId}, SessionId={SessionId}, NewTitle={NewTitle}",
                correlationId, userId, sessionId, request.NewTitle);

            try
            {
                await _aiService.UpdateSessionTitleAsync(sessionId, userId, request.NewTitle);
                return Ok("Session updated successfully");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update title for session {SessionId}", sessionId);
                return StatusCode(500, new { error = "Internal server error." });
            }
        }

        [Authorize]
        [HttpPost("feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] AIQueryFeedbackDto feedbackDto)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = SetCorrelationIdHeader();
            _logger.LogInformation("SubmitFeedback called. CorrelationId={CorrelationId}, UserId={UserId}, Vote={Vote}",
                correlationId, userId, feedbackDto.Vote);

            try
            {
                var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var result = await _aiService.SubmitFeedbackAsync(feedbackDto, correlationId, jwtToken);
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Feedback submission failed. CorrelationId={CorrelationId}", correlationId);
                return StatusCode(500, new { error = ex.Message, correlationId });
            }
        }

        /// <summary>
        /// Retrieves the top popular queries from the FastAPI service.
        /// </summary>
        [Authorize]
        [HttpGet("popular-queries")]
        public async Task<ActionResult<List<PopularQueryDto>>> GetPopularQueries([FromQuery] int limit = 10)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            if (limit < 1 || limit > 50)
                return BadRequest(new { error = "Limit must be between 1 and 50." });

            var correlationId = SetCorrelationIdHeader();
            _logger.LogInformation("GetPopularQueries called. CorrelationId={CorrelationId}, UserId={UserId}, Limit={Limit}",
                correlationId, userId, limit);

            try
            {
                var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
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