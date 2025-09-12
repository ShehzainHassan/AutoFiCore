using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
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
    /// <summary>
    /// Provides AI assistant operations such as querying AI service, managing chat sessions,
    /// generating suggestions, and handling feedback.
    /// </summary>
    public class AIAssistantService : IAIAssistantService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IUnitOfWork _uow;
        private readonly IUserContextService _userContextService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AIAssistantService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AIAssistantService"/> class.
        /// </summary>
        public AIAssistantService(IHttpClientFactory httpClientFactory, IUnitOfWork uow, ILogger<AIAssistantService> logger, IServiceScopeFactory scopeFactory, IUserContextService userContextService, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _uow = uow;
            _logger = logger;
            _userContextService = userContextService;
            _cache = cache;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Sends a query to the external FastAPI service and persists the chat history.
        /// </summary>
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

        /// <summary>
        /// Retrieves AI-powered suggestions for a user, combining ML and .NET context.
        /// Uses caching to improve performance.
        /// </summary>
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
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var strategy = uow.DbContext.Database.CreateExecutionStrategy();

            try
            {
                string sessionId = null!;

                await strategy.ExecuteAsync(async () =>
                {
                    await uow.BeginTransactionAsync();
                    try
                    {
                        sessionId = await uow.ChatRepository.SaveChatHistoryAsync(payload, result);

                        await uow.SaveChangesAsync();
                        await uow.CommitTransactionAsync();
                    }
                    catch
                    {
                        await uow.RollbackTransactionAsync();
                        throw;
                    }
                });

                return sessionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist chat history");
                return null!;
            }
        }

        /// <summary>
        /// Gets all chat titles for a given user.
        /// </summary>
        public async Task<List<ChatTitleDto>> GetChatTitlesAsync(int userId)
        {
            var chats = await _uow.ChatRepository.GetUserChatsAsync(userId);
            return chats;
        }

        /// <summary>
        /// Retrieves the full chat session details for the given user and session.
        /// </summary>
        public async Task<ChatSessionDto?> GetFullChatAsync(int userId, string sessionId)
        {
            var chat = await _uow.ChatRepository.GetSessionAsync(sessionId, userId);
            return chat == null ? null : ChatMapper.ToDto(chat);
        }

        /// <summary>
        /// Submits feedback about an AI query to the external FastAPI service.
        /// </summary>
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

        /// <summary>
        /// Retrieves a list of popular queries from the external FastAPI service.
        /// </summary>
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

        /// <summary>
        /// Deletes a specific chat session for a user inside a transaction.
        /// </summary>
        public async Task DeleteSessionAsync(string sessionId, int userId)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var strategy = uow.DbContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    await uow.BeginTransactionAsync();

                    try
                    {
                        await uow.ChatRepository.DeleteSessionAsync(sessionId, userId);
                        await uow.SaveChangesAsync();
                        await uow.CommitTransactionAsync();

                        _logger.LogInformation($"Deleted session {sessionId} for user {userId}.");
                    }
                    catch
                    {
                        await uow.RollbackTransactionAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting session {sessionId} for user {userId}.");
                throw;
            }
        }

        /// <summary>
        /// Deletes all chat sessions for a user inside a transaction.
        /// </summary>
        public async Task DeleteAllSessionsAsync(int userId)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var strategy = uow.DbContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    await uow.BeginTransactionAsync();
                    try
                    {
                        await uow.ChatRepository.DeleteAllSessionsAsync(userId);
                        await uow.SaveChangesAsync();
                        await uow.CommitTransactionAsync();

                        _logger.LogInformation($"Deleted all sessions for user {userId}.");
                    }
                    catch
                    {
                        await uow.RollbackTransactionAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting all sessions for user {userId}.");
                throw;
            }
        }

        /// <summary>
        /// Updates the title of a chat session for a user inside a transaction.
        /// </summary>
        public async Task UpdateSessionTitleAsync(string sessionId, int userId, string newTitle)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var strategy = uow.DbContext.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    await uow.BeginTransactionAsync();
                    try
                    {
                        var session = await uow.ChatRepository.GetSessionAsync(sessionId, userId);

                        if (session == null)
                        {
                            _logger.LogWarning(
                                $"UpdateSessionTitleAsync: Session {sessionId} not found for user {userId}."
                            );
                            await uow.RollbackTransactionAsync();
                            return;
                        }

                        session.Title = newTitle;
                        session.UpdatedAt = DateTime.UtcNow;

                        await uow.ChatRepository.UpdateSessionAsync(session);
                        await uow.SaveChangesAsync();
                        await uow.CommitTransactionAsync();

                        _logger.LogInformation(
                            $"Session {sessionId} title successfully updated to '{newTitle}' for user {userId}."
                        );
                    }
                    catch
                    {
                        await uow.RollbackTransactionAsync();
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating title for session {sessionId}.");
                throw;
            }
        }
    }
}