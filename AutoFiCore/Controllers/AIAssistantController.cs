using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="AIAssistantController"/> class.
        /// </summary>
        /// <param name="aiService">Service for handling AI assistant operations.</param>
        /// <param name="userContextService">Service for retrieving user context data.</param>
        /// <param name="logger">Logger for tracking AI assistant activity.</param>
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
        /// <param name="payload">The query payload containing the question and context.</param>
        /// <returns>An <see cref="AIResponseModel"/> containing the AI's response.</returns>
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

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("GetUserContext called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var context = await _userContextService.GetUserContextAsync(userId);
            return Ok(context);
        }

        /// <summary>
        /// Retrieves contextual AI-generated suggestions based on the user's history.
        /// </summary>
        /// <param name="userId">The user ID for which suggestions are requested.</param>
        /// <returns>A list of personalized suggestions.</returns>
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
            var suggestions = await _aiService.GetSuggestionsAsync(userId, correlationId, jwtToken);

            return Ok(suggestions);
        }

        /// <summary>
        /// Retrieves the list of chat titles for the current user.
        /// </summary>
        /// <returns>A list of chat session titles.</returns>
        [Authorize]
        [HttpGet("chats")]
        public async Task<IActionResult> GetChatTitles()
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("GetChatTitles called. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);

            var titles = await _aiService.GetChatTitlesAsync(userId);
            return Ok(titles);
        }

        /// <summary>
        /// Retrieves the full chat history for a specific session.
        /// </summary>
        /// <param name="sessionId">The session ID of the chat.</param>
        /// <returns>The full chat history or NotFound if not found.</returns>
        [Authorize]
        [HttpGet("chats/{sessionId}")]
        public async Task<IActionResult> GetChat(string sessionId)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("GetChat called. CorrelationId={CorrelationId}, UserId={UserId}, SessionId={SessionId}", correlationId, userId, sessionId);

            var chat = await _aiService.GetFullChatAsync(userId, sessionId);
            return chat != null ? Ok(chat) : NotFound(new { error = "Chat not found." });
        }

        /// <summary>
        /// Deletes a specific chat session by ID.
        /// </summary>
        /// <param name="sessionId">The session ID of the chat to delete.</param>
        [Authorize]
        [HttpDelete("chats/{sessionId}")]
        public async Task<IActionResult> DeleteSession(string sessionId)
        {
            if (!IsUserContextValid(out var userId))
                return Unauthorized(new { error = "Missing user context." });

            var correlationId = GetCorrelationId()!;
            _logger.LogInformation("DeleteSession called. CorrelationId={CorrelationId}, UserId={UserId}, SessionId={SessionId}", correlationId, userId, sessionId);

            await _aiService.DeleteSessionAsync(sessionId, userId);
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

            await _aiService.DeleteAllSessionsAsync(userId);
            return NoContent();
        }

        /// <summary>
        /// Updates the title of a chat session.
        /// </summary>
        /// <param name="sessionId">The session ID of the chat to update.</param>
        /// <param name="request">The new title payload.</param>
        /// <returns>OK if successful, NotFound if session not found.</returns>
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

            await _aiService.UpdateSessionTitleAsync(sessionId, userId, request.NewTitle);
            return Ok("Session updated successfully");
        }

        /// <summary>
        /// Submits user feedback for a specific AI query response.
        /// </summary>
        /// <param name="feedbackDto">The feedback details including vote and query info.</param>
        /// <returns>The result of feedback submission.</returns>
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
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the top popular queries from the AI service.
        /// </summary>
        /// <param name="limit">Maximum number of queries to return (1–50).</param>
        /// <returns>A list of popular queries.</returns>
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
            var queries = await _aiService.GetPopularQueriesAsync(limit, correlationId, jwtToken);
            return Ok(queries);
        }
    }
}
