using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AutoFiCore.Services
{
    public interface IAIAssistantService
    {
        Task<AIResponseModel> QueryFastApiAsync(EnrichedAIQuery payload, string correlationId, string jwtToken);
        Task<object> GetSuggestionsAsync(int userId);
        Task<List<ChatTitleDto>> GetChatTitlesAsync(int userId);
        Task<ChatSessionDto?> GetFullChatAsync(int userId, string sessionId);
    }

    public class AIAssistantService : IAIAssistantService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IUnitOfWork _uow;
        private readonly IUserContextCache _userContextCache;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<AIAssistantService> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreaker;

        public AIAssistantService(IHttpClientFactory httpClientFactory, IUnitOfWork uow, ILogger<AIAssistantService> logger, IServiceScopeFactory scopeFactory, IUserContextCache userContextCache, IUserContextService userContextService)
        {
            _httpClientFactory = httpClientFactory;
            _uow = uow;
            _logger = logger;
            _userContextCache = userContextCache;
            _userContextService = userContextService;

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(2));

            _circuitBreaker = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1));

            _scopeFactory = scopeFactory;

        }
        public async Task<AIResponseModel> QueryFastApiAsync(EnrichedAIQuery payload, string correlationId, string jwtToken)
        {
            var client = _httpClientFactory.CreateClient("FastApi");
            client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var userContext = await _userContextCache.GetOrAddAsync(payload.UserId, async () =>
            {
                return await _userContextService.GetUserContextAsync(payload.UserId);
            });

            var enrichedPayload = new
            {
                query = new
                {
                    user_id = payload.UserId,
                    question = payload.Query.Question
                },
                context = userContext,
                session_id = payload.SessionId,
            };

            var content = new StringContent(
                JsonSerializer.Serialize(enrichedPayload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("query", content);
            var resultJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("FastAPI query failed. CorrelationId={CorrelationId}", correlationId);
                return new AIResponseModel { Answer = "AI service unavailable.", UiType = "TEXT" };
            }

            var result = JsonSerializer.Deserialize<AIResponseModel>(resultJson)!;

            _ = Task.Run(() => SaveChatHistoryAsync(payload, result));

            return result;
        }
        public async Task<object> GetSuggestionsAsync(int userId)
        {
            return new
            {
                //TO DO
            };
        }
        private async Task SaveChatHistoryAsync(EnrichedAIQuery payload, AIResponseModel result)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // Generate a new session ID if none is provided
                if (string.IsNullOrWhiteSpace(payload.SessionId))
                {
                    payload.SessionId = Guid.NewGuid().ToString();
                }

                // Try to get existing session
                var session = await uow.ChatRepository.GetSessionAsync(payload.SessionId, payload.UserId);

                if (session == null)
                {
                    string GenerateTitle(string question)
                    {
                        if (string.IsNullOrWhiteSpace(question)) return "New Chat";
                        var title = question.Length > 50 ? question[..50] : question;
                        title = title.Replace("\r", " ").Replace("\n", " ").Trim();
                        return title;
                    }

                    // Create new session
                    session = new ChatSession
                    {
                        Id = payload.SessionId,
                        UserId = payload.UserId,
                        CreatedAt = DateTime.UtcNow,
                        Title = GenerateTitle(payload.Query.Question),
                        Messages = new List<ChatMessage>()
                    };

                    await uow.ChatRepository.AddSessionAsync(session);
                }

                // Add user message
                var userMessage = new ChatMessage
                {
                    ChatSessionId = session.Id,
                    Sender = "User",
                    Message = payload.Query.Question,
                    Timestamp = DateTime.UtcNow
                };
                await uow.ChatRepository.AddMessageAsync(userMessage);

                // Add AI message
                var aiMessage = new ChatMessage
                {
                    ChatSessionId = session.Id,
                    Sender = "AI",
                    Message = result.Answer,
                    Timestamp = DateTime.UtcNow,
                    UiType = result.UiType,
                    QueryType = result.QueryType,
                    SuggestedActions = result.SuggestedActions,
                    Sources = result.Sources
                };
                await uow.ChatRepository.AddMessageAsync(aiMessage);

                // Save changes
                await uow.ChatRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist chat history");
            }
        }
        public async Task<List<ChatTitleDto>> GetChatTitlesAsync(int userId)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var chats = await uow.ChatRepository.GetUserChatsAsync(userId);
            return chats.Select(c => new ChatTitleDto
            {
                Id = c.Id,
                Title = c.Title,
                CreatedAt = c.CreatedAt
            }).ToList();
        }
        public async Task<ChatSessionDto?> GetFullChatAsync(int userId, string sessionId)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var chat = await uow.ChatRepository.GetSessionAsync(sessionId, userId);
            if (chat == null) return null;

            return new ChatSessionDto
            {
                Id = chat.Id,
                Title = chat.Title,
                CreatedAt = chat.CreatedAt,
                Messages = chat.Messages.Select(m => new ChatMessageDto
                {
                    Sender = m.Sender,
                    Message = m.Message,
                    Timestamp = m.Timestamp
                }).ToList()
            };
        }
    }
}