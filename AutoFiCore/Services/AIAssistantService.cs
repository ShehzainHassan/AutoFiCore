using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoFiCore.Services
{
    public interface IAIAssistantService
    {
        Task<AIResponseModel> QueryFastApiAsync(EnrichedAIQuery payload, string correlationId, string jwtToken);
        Task<List<ChatTitleDto>> GetChatTitlesAsync(int userId);
        Task<ChatSessionDto?> GetFullChatAsync(int userId, string sessionId);
        Task<FeedbackResponseDto> SubmitFeedbackAsync(AIQueryFeedbackDto feedbackDto, string correlationId, string jwtToken);
        Task<List<string>> GetSuggestionsAsync(int userId, string correlationId, string jwtToken);
        Task<List<PopularQueryDto>> GetPopularQueriesAsync(int limit, string correlationId, string jwtToken);
        Task DeleteSessionAsync(string sessionId, int userId);
        Task DeleteAllSessionsAsync(int userId);
        Task UpdateSessionTitleAsync(string sessionId, int userId, string newTitle);
    }

    public class AIAssistantService : IAIAssistantService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IUnitOfWork _uow;
        private readonly IUserContextService _userContextService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AIAssistantService> _logger;
        public AIAssistantService(IHttpClientFactory httpClientFactory, IUnitOfWork uow, ILogger<AIAssistantService> logger, IServiceScopeFactory scopeFactory, IUserContextService userContextService, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _uow = uow;
            _logger = logger;
            _userContextService = userContextService;
            _cache = cache;
            _scopeFactory = scopeFactory;
        }
        public async Task<AIResponseModel> QueryFastApiAsync(EnrichedAIQuery payload, string correlationId, string jwtToken)
        {
            var client = _httpClientFactory.CreateClient("FastApi");
            client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var userContext = await _userContextService.GetUserContextAsync(payload.UserId);

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

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("query", content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FastAPI query request failed. CorrelationId={CorrelationId}", correlationId);
                return new AIResponseModel { Answer = "AI service unavailable.", UiType = "TEXT" };
            }

            var resultJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("FastAPI query failed. StatusCode={StatusCode}, CorrelationId={CorrelationId}", response.StatusCode, correlationId);
                return new AIResponseModel { Answer = "AI service unavailable.", UiType = "TEXT" };
            }

            var result = JsonSerializer.Deserialize<AIResponseModel>(resultJson)!;

            var sessionId = await SaveChatHistoryAsync(payload, result);
            result.SessionId = sessionId;

            return result;
        }
        private async Task<List<string>> GenerateHybridSuggestions(MLUserContextDto? mlContext, UserContextDTO? dotNetContext)
        {
            var suggestions = new List<string>();

            var vehicleCache = new Dictionary<int, Vehicle>();
            var auctionCache = new Dictionary<int, Auction>();

            async Task<Vehicle?> GetVehicleCachedAsync(int vehicleId)
            {
                if (!vehicleCache.ContainsKey(vehicleId))
                {
                    var v = await _uow.Vehicles.GetVehicleByIdAsync(vehicleId);
                    if (v != null) vehicleCache[vehicleId] = v;
                }
                return vehicleCache.GetValueOrDefault(vehicleId);
            }

            async Task<Auction?> GetAuctionCachedAsync(int auctionId)
            {
                if (!auctionCache.ContainsKey(auctionId))
                {
                    var a = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
                    if (a != null) auctionCache[auctionId] = a;
                }
                return auctionCache.GetValueOrDefault(auctionId);
            }

            if (mlContext != null)
            {
                foreach (var interaction in mlContext.UserInteractions.Take(5))
                {
                    var vehicle = await GetVehicleCachedAsync(interaction.VehicleId);
                    if (vehicle != null)
                    {
                        suggestions.Add($"I would like to see more details about the {vehicle.Make} {vehicle.Model} {vehicle.Year}");
                        suggestions.Add($"I would like to explore similar cars to the {vehicle.Make} {vehicle.Model} {vehicle.Year}");
                    }
                }

                foreach (var evt in mlContext.AnalyticsEvents.Take(5))
                {
                    var auction = await GetAuctionCachedAsync(evt.AuctionId);
                    if (auction?.Vehicle != null)
                    {
                        var v = auction.Vehicle;
                        suggestions.Add($"I would like more details on the auction for {v.Make} {v.Model} {v.Year}");
                        suggestions.Add($"I want to know the current bid trends for {v.Make} {v.Model} {v.Year}");
                    }
                }
            }

            if (dotNetContext != null)
            {
                foreach (var auction in dotNetContext.AuctionHistory.Take(5))
                {
                    if (auction != null)
                    {
                        var vehicle = await GetVehicleCachedAsync(auction.VehicleId);
                        if (vehicle != null)
                        {
                            suggestions.Add($"I would like updates on upcoming auctions for {vehicle.Make} {vehicle.Model} {vehicle.Year}");
                            suggestions.Add($"I want to know if {vehicle.Make} {vehicle.Model} {vehicle.Year} is still available");
                        }
                    }
                }

                foreach (var search in dotNetContext.SavedSearches.Take(5))
                {
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        suggestions.Add($"I want to look for cars matching '{search}'");
                        suggestions.Add($"I would like similar vehicles to '{search}'");
                    }
                }
            }

            if (suggestions.Count == 0)
            {
                _logger.LogWarning("No valid context found for suggestions.");
                return new List<string>();
            }

            return suggestions.Distinct().Take(10).ToList();
        }
        public async Task<List<string>> GetSuggestionsAsync(int userId, string correlationId, string jwtToken)
        {
            var cacheKey = $"suggestions:{userId}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cachedSuggestions) && cachedSuggestions != null)
            {
                _logger.LogInformation("Returning cached suggestions. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);
                return cachedSuggestions;
            }

            var mlContext = await _userContextService.FetchMLContextAsync(userId, correlationId, jwtToken);
            var dotNetContext = await _userContextService.GetUserContextAsync(userId);

            var suggestions = await GenerateHybridSuggestions(mlContext, dotNetContext);

            _cache.Set(cacheKey, suggestions, TimeSpan.FromMinutes(10));
            _logger.LogInformation("Suggestions cached. CorrelationId={CorrelationId}, Count={Count}", correlationId, suggestions.Count);

            return suggestions;
        }
        private async Task<string> SaveChatHistoryAsync(EnrichedAIQuery payload, AIResponseModel result)
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

                    session = new ChatSession
                    {
                        Id = payload.SessionId,
                        UserId = payload.UserId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Title = GenerateTitle(payload.Query.Question),
                        Messages = new List<ChatMessage>()
                    };

                    await uow.ChatRepository.AddSessionAsync(session);
                }
                else
                {
                    session.UpdatedAt = DateTime.UtcNow;
                    await uow.ChatRepository.UpdateSessionAsync(session);
                }

                var userMessage = new ChatMessage
                {
                    ChatSessionId = session.Id,
                    Sender = "User",
                    Message = payload.Query.Question,
                    Timestamp = DateTime.UtcNow
                };
                await uow.ChatRepository.AddMessageAsync(userMessage);

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

                await uow.ChatRepository.SaveChangesAsync();
                return session.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist chat history");
                return null!;
            }
        }
        public async Task<List<ChatTitleDto>> GetChatTitlesAsync(int userId)
        {
            var chats = await _uow.ChatRepository.GetUserChatsAsync(userId);
            return chats;
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
                UpdatedAt = chat.UpdatedAt,
                Messages = chat.Messages
                    .Select(m => new ChatMessageDto
                    {
                        Id = m.Id,
                        Sender = m.Sender,
                        Message = m.Message,
                        Timestamp = m.Timestamp,
                        Feedback = m.Feedback,
                        UiType = m.UiType,
                        QueryType = m.QueryType,
                        SuggestedActions = m.SuggestedActions ?? new List<string>(),
                        Sources = m.Sources ?? new List<string>()
                    })
                    .OrderBy(m => m.Id)
                    .ToList()
            };
        }
        public async Task<FeedbackResponseDto> SubmitFeedbackAsync(AIQueryFeedbackDto feedbackDto, string correlationId, string jwtToken)
        {
            var client = _httpClientFactory.CreateClient("FastApi");
            client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var payload = new
            {
                message_id = feedbackDto.MessageId,
                vote = feedbackDto.Vote.ToString().ToUpperInvariant()
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            _logger.LogInformation("Submitting feedback to FastAPI: MessageId={MessageId}, Vote={Vote}, CorrelationId={CorrelationId}",
                feedbackDto.MessageId, feedbackDto.Vote, correlationId);

            var response = await client.PostAsync("feedback", content);
            var resultJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("FastAPI feedback submission failed. StatusCode={StatusCode}, Response={Response}, CorrelationId={CorrelationId}",
                    response.StatusCode, resultJson, correlationId);
                throw new HttpRequestException($"Feedback submission failed: {resultJson}");
            }

            var result = JsonSerializer.Deserialize<FeedbackResponseDto>(resultJson)!;

            _logger.LogInformation("Feedback submitted successfully: MessageId={MessageId}, Feedback={Feedback}, CorrelationId={CorrelationId}",
                result.MessageId, result.Feedback, correlationId);

            return result;
        }
        public async Task<List<PopularQueryDto>> GetPopularQueriesAsync(int limit, string correlationId, string jwtToken)
        {
            var client = _httpClientFactory.CreateClient("FastApi");
            client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await client.GetAsync($"popular-queries?limit={limit}");
            var resultJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("FastAPI popular queries failed. CorrelationId={CorrelationId}, StatusCode={StatusCode}",
                    correlationId, response.StatusCode);
                throw new HttpRequestException($"FastAPI request failed with status code {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<List<PopularQueryDto>>(resultJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? new List<PopularQueryDto>();
        }
        public async Task DeleteSessionAsync(string sessionId, int userId)
        {
            try
            {
                await _uow.ChatRepository.DeleteSessionAsync(sessionId, userId);
                _logger.LogInformation($"Deleted session {sessionId} for user {userId}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting session {sessionId} for user {userId}.");
                throw;
            }
        }
        public async Task DeleteAllSessionsAsync(int userId)
        {
            try
            {
                await _uow.ChatRepository.DeleteAllSessionsAsync(userId);
                _logger.LogInformation($"Deleted all sessions for user {userId}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting all sessions for user {userId}.");
                throw;
            }
        }
        public async Task UpdateSessionTitleAsync(string sessionId, int userId, string newTitle)
        {
            var session = await _uow.ChatRepository.GetSessionAsync(sessionId, userId);

            if (session == null)
            {
                _logger.LogWarning(
                    $"UpdateSessionTitleAsync: Session {sessionId} not found for user {userId}."
                );
                return;
            }

            session.Title = newTitle;
            session.UpdatedAt = DateTime.UtcNow;

            await _uow.ChatRepository.UpdateSessionAsync(session);

            _logger.LogInformation(
                $"Session {sessionId} title successfully updated to '{newTitle}' for user {userId}."
            );
        }
    }
}