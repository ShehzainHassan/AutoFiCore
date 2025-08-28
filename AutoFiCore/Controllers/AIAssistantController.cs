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
        private readonly IUserContextCache _userContextCache;
        private readonly ILogger<AIAssistantController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AIAssistantController"/> class.
        /// </summary>
        /// <param name="aiService">Service for handling AI assistant operations.</param>
        /// <param name="logger">Logger for tracking AI assistant activity.</param>

        public AIAssistantController(IAIAssistantService aiService, IUserContextService userContextService, ILogger<AIAssistantController> logger, IUserContextCache userContextCache)
        {
            _aiService = aiService;
            _userContextService = userContextService;
            _logger = logger;
            _userContextCache = userContextCache;
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "Missing user context." });

            var context = await _userContextCache.GetOrAddAsync(int.Parse(userId), ()=> 
            _userContextService.GetUserContextAsync(int.Parse(userId)));
            return Ok(context);
        }

        /// <summary>
        /// Retrieves contextual AI-generated suggestions based on the user's auction and vehicle history.
        /// </summary>
        /// <returns>List of personalized suggestions from the AI assistant.</returns>
        [HttpGet("suggestions")]
        [Authorize]
        public async Task<IActionResult> GetSuggestions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "Missing user context." });

            var suggestions = await _aiService.GetSuggestionsAsync(int.Parse(userId));
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

    }
}