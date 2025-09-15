using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Services;
using AutoFiCore.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFiCore.Controllers
{
    /// <summary>
    /// Controller for interfacing with the AI Assistant service.
    /// Provides endpoints for querying AI, retrieving user context, managing chats, 
    /// and generating personalized AI-driven suggestions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AIAssistantController : SecureControllerBase
    {
        private readonly IAIAssistantService _aiService;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<AIAssistantController> _logger;

        public AIAssistantController(
            IAIAssistantService aiService,
            IUserContextService userContextService,
            ILogger<AIAssistantController> logger)
        {
            _aiService = aiService;
            _userContextService = userContextService;
            _logger = logger;
        }

        /// <summary>
        /// Sends a query payload to the AI service and returns the generated response.
        /// </summary>
        [Authorize]
        [HttpPost("query")]
        public async Task<ActionResult<AIResponseModel>> QueryAI([FromBody] EnrichedAIQuery payload)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            if (payload.UserId != userId)
                return Forbid("User ID mismatch between token and payload.");

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("AI Query initiated. CorrelationId={CorrelationId}, UserId={UserId}, Question={Question}",
                correlationId, userId, payload.Query.Question);

            var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _aiService.QueryFastApiAsync(payload, correlationId, jwtToken);

            if (!result.IsSuccess)
            {
                _logger.LogError("AI Query failed. CorrelationId={CorrelationId}, Error={Error}", correlationId, result.Error);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = result.Error });
            }

            _logger.LogInformation("AI Query completed. CorrelationId={CorrelationId}, Answer={Answer}", correlationId, result.Value!.Answer);
            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the user's auction and vehicle history to provide context for AI suggestions.
        /// </summary>
        [Authorize]
        [HttpGet("user-context")]
        public async Task<IActionResult> GetUserContext()
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("GetUserContext called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var result = await _userContextService.GetUserContextAsync(userId);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves contextual AI-generated suggestions based on the user's history.
        /// </summary>
        [Authorize]
        [HttpGet("contextual-suggestions/{userId}")]
        public async Task<IActionResult> GetContextualSuggestions(int userId)
        {
            if (!IsUserContextValid(out var tokenUserId))
                return Unauthorized(new { error = "Missing user context." });

            if (userId != tokenUserId)
                return Forbid("User ID mismatch between token and route.");

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("Contextual suggestion request. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _aiService.GetSuggestionsAsync(userId, correlationId, jwtToken);

            if (!result.IsSuccess)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the list of chat titles for the current user.
        /// </summary>
        [Authorize]
        [HttpGet("chats")]
        public async Task<IActionResult> GetChatTitles()
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("GetChatTitles called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var result = await _aiService.GetChatTitlesAsync(userId);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the full chat history for a specific session.
        /// </summary>
        [Authorize]
        [HttpGet("chats/{sessionId}")]
        public async Task<IActionResult> GetChat(string sessionId)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("GetChat called. CorrelationId={CorrelationId}, UserId={UserId}, SessionId={SessionId}", correlationId, userId, sessionId);

            var result = await _aiService.GetFullChatAsync(userId, sessionId);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes a specific chat session by ID.
        /// </summary>
        [Authorize]
        [HttpDelete("chats/{sessionId}")]
        public async Task<IActionResult> DeleteSession(string sessionId)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("DeleteSession called. CorrelationId={CorrelationId}, UserId={UserId}, SessionId={SessionId}", correlationId, userId, sessionId);

            var result = await _aiService.DeleteSessionAsync(sessionId, userId);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return NoContent();
        }

        /// <summary>
        /// Deletes all chat sessions for the current user.
        /// </summary>
        [Authorize]
        [HttpDelete("chats")]
        public async Task<IActionResult> DeleteAllSessions()
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("DeleteAllSessions called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var result = await _aiService.DeleteAllSessionsAsync(userId);

            if (!result.IsSuccess)
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });

            return NoContent();
        }

        /// <summary>
        /// Updates the title of a chat session.
        /// </summary>
        [Authorize]
        [HttpPut("chats/{sessionId}/title")]
        public async Task<IActionResult> UpdateSessionTitle(string sessionId, [FromBody] UpdateChatTitle request)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            if (string.IsNullOrWhiteSpace(request.NewTitle))
                return BadRequest(new { error = "Title cannot be empty." });

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("UpdateSessionTitle called. CorrelationId={CorrelationId}, UserId={UserId}, SessionId={SessionId}, NewTitle={NewTitle}",
                correlationId, userId, sessionId, request.NewTitle);

            var result = await _aiService.UpdateSessionTitleAsync(sessionId, userId, request.NewTitle);

            if (!result.IsSuccess)
                return NotFound(new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Submits user feedback for a specific AI query response.
        /// </summary>
        [Authorize]
        [HttpPost("feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] AIQueryFeedbackDto feedbackDto)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("SubmitFeedback called. CorrelationId={CorrelationId}, UserId={UserId}, Vote={Vote}",
                correlationId, userId, feedbackDto.Vote);

            var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _aiService.SubmitFeedbackAsync(feedbackDto, correlationId, jwtToken);

            if (!result.IsSuccess)
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });

            return Ok(result.Value);
        }

        /// <summary>
        /// Retrieves the top popular queries from the AI service.
        /// </summary>
        [Authorize]
        [HttpGet("popular-queries")]
        public async Task<ActionResult<List<PopularQueryDto>>> GetPopularQueries([FromQuery] int limit = 10)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            if (limit < 1 || limit > 50)
                return BadRequest(new { error = "Limit must be between 1 and 50." });

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("GetPopularQueries called. CorrelationId={CorrelationId}, UserId={UserId}, Limit={Limit}",
                correlationId, userId, limit);

            var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _aiService.GetPopularQueriesAsync(limit, correlationId, jwtToken);

            if (!result.IsSuccess)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = result.Error });

            return Ok(result.Value);
        }
    }
}
