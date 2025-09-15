using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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

        private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Initializes a new instance of <see cref="AIAssistantService"/>.
        /// </summary>
        public AIAssistantService(
            IHttpClientFactory httpClientFactory,
            IUnitOfWork uow,
            ILogger<AIAssistantService> logger,
            IServiceScopeFactory scopeFactory,
            IUserContextService userContextService,
            IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Sends a query to the external FastAPI service and persists the chat history.
        /// </summary>
        public async Task<Result<AIResponseModel>> QueryFastApiAsync(EnrichedAIQuery payload, string correlationId, string jwtToken)
        {
            var client = _httpClientFactory.CreateClient("FastApi");
            client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var userContextResult = await _userContextService.GetUserContextAsync(payload.UserId);
            var userContext = userContextResult.IsSuccess ? userContextResult.Value : null;

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
                JsonSerializer.Serialize(enrichedPayload, _serializerOptions),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await client.PostAsync("query", content);
                var resultJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("FastAPI query failed. StatusCode={StatusCode}, CorrelationId={CorrelationId}", response.StatusCode, correlationId);
                    return Result<AIResponseModel>.Failure($"FastAPI returned {response.StatusCode}");
                }

                var result = JsonSerializer.Deserialize<AIResponseModel>(resultJson, _serializerOptions)!;
                var sessionId = await SaveChatHistoryAsync(payload, result);
                result.SessionId = sessionId;

                return Result<AIResponseModel>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FastAPI query request failed. CorrelationId={CorrelationId}", correlationId);
                return Result<AIResponseModel>.Failure("AI service unavailable.");
            }
        }

        /// <summary>
        /// Retrieves AI-powered suggestions for a user, combining ML and .NET context.
        /// </summary>
        public async Task<Result<List<string>>> GetSuggestionsAsync(int userId, string correlationId, string jwtToken)
        {
            var cacheKey = $"suggestions:{userId}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cachedSuggestions) && cachedSuggestions != null)
            {
                _logger.LogInformation("Returning cached suggestions. CorrelationId={CorrelationId}, UserId={UserId}", correlationId, userId);
                return Result<List<string>>.Success(cachedSuggestions);
            }

            try
            {
                var mlContextResult = await _userContextService.FetchMLContextAsync(userId, correlationId, jwtToken);
                var mlContext = mlContextResult.IsSuccess ? mlContextResult.Value : null;

                var dotNetContextResult = await _userContextService.GetUserContextAsync(userId);
                var dotNetContext = dotNetContextResult.IsSuccess ? dotNetContextResult.Value : null;

                var suggestions = await GenerateHybridSuggestions(mlContext, dotNetContext);

                _cache.Set(cacheKey, suggestions, TimeSpan.FromMinutes(10));
                _logger.LogInformation("Suggestions cached. CorrelationId={CorrelationId}, Count={Count}", correlationId, suggestions.Count);

                return Result<List<string>>.Success(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate suggestions. CorrelationId={CorrelationId}", correlationId);
                return Result<List<string>>.Failure("Failed to generate suggestions.");
            }
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
                    var vehicle = await GetVehicleCachedAsync(auction.VehicleId);
                    if (vehicle != null)
                    {
                        suggestions.Add($"I would like updates on upcoming auctions for {vehicle.Make} {vehicle.Model} {vehicle.Year}");
                        suggestions.Add($"I want to know if {vehicle.Make} {vehicle.Model} {vehicle.Year} is still available");
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

            return suggestions.Distinct().Take(10).ToList();
        }

        /// <summary>
        /// Persists chat history to the database in a transaction.
        /// </summary>
        private async Task<string?> SaveChatHistoryAsync(EnrichedAIQuery payload, AIResponseModel result)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var strategy = uow.DbContext.Database.CreateExecutionStrategy();

            try
            {
                string? sessionId = null;

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
                return null;
            }
        }

        /// <summary>
        /// Gets chat titles for a user.
        /// </summary>
        public async Task<Result<List<ChatTitleDto>>> GetChatTitlesAsync(int userId)
        {
            try
            {
                var chats = await _uow.ChatRepository.GetUserChatsAsync(userId);
                return Result<List<ChatTitleDto>>.Success(chats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch chat titles for user {UserId}", userId);
                return Result<List<ChatTitleDto>>.Failure("Failed to fetch chat titles.");
            }
        }

        /// <summary>
        /// Gets the full chat session with messages for a user.
        /// </summary>
        public async Task<Result<ChatSessionDto?>> GetFullChatAsync(int userId, string sessionId)
        {
            try
            {
                var chat = await _uow.ChatRepository.GetSessionAsync(sessionId, userId);
                return Result<ChatSessionDto?>.Success(chat == null ? null : ChatMapper.ToDto(chat));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch full chat. SessionId={SessionId}, UserId={UserId}", sessionId, userId);
                return Result<ChatSessionDto?>.Failure("Failed to fetch chat session.");
            }
        }

        /// <summary>
        /// Submits feedback on an AI response to FastAPI.
        /// </summary>
        public async Task<Result<FeedbackResponseDto>> SubmitFeedbackAsync(AIQueryFeedbackDto feedbackDto, string correlationId, string jwtToken)
        {
            var client = _httpClientFactory.CreateClient("FastApi");
            client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var payload = new
            {
                message_id = feedbackDto.MessageId,
                vote = feedbackDto.Vote.ToString().ToUpperInvariant()
            };

            var content = new StringContent(JsonSerializer.Serialize(payload, _serializerOptions), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("feedback", content);
                var resultJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("FastAPI feedback submission failed. StatusCode={StatusCode}, Response={Response}, CorrelationId={CorrelationId}",
                        response.StatusCode, resultJson, correlationId);
                    return Result<FeedbackResponseDto>.Failure("Feedback submission failed.");
                }

                var result = JsonSerializer.Deserialize<FeedbackResponseDto>(resultJson, _serializerOptions)!;
                return Result<FeedbackResponseDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting feedback. CorrelationId={CorrelationId}", correlationId);
                return Result<FeedbackResponseDto>.Failure("Error submitting feedback.");
            }
        }

        /// <summary>
        /// Fetches popular queries from FastAPI.
        /// </summary>
        public async Task<Result<List<PopularQueryDto>>> GetPopularQueriesAsync(int limit, string correlationId, string jwtToken)
        {
            var client = _httpClientFactory.CreateClient("FastApi");
            client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                var response = await client.GetAsync($"popular-queries?limit={limit}");
                var resultJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("FastAPI popular queries failed. CorrelationId={CorrelationId}, StatusCode={StatusCode}", correlationId, response.StatusCode);
                    return Result<List<PopularQueryDto>>.Failure("Failed to fetch popular queries.");
                }

                var result = JsonSerializer.Deserialize<List<PopularQueryDto>>(resultJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return Result<List<PopularQueryDto>>.Success(result ?? new List<PopularQueryDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching popular queries. CorrelationId={CorrelationId}", correlationId);
                return Result<List<PopularQueryDto>>.Failure("Error fetching popular queries.");
            }
        }

        /// <summary>
        /// Deletes a specific chat session for a user.
        /// </summary>
        public async Task<Result<bool>> DeleteSessionAsync(string sessionId, int userId)
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
                    }
                    catch
                    {
                        await uow.RollbackTransactionAsync();
                        throw;
                    }
                });

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session {SessionId} for user {UserId}", sessionId, userId);
                return Result<bool>.Failure("Error deleting session.");
            }
        }

        /// <summary>
        /// Deletes all chat sessions for a user.
        /// </summary>
        public async Task<Result<bool>> DeleteAllSessionsAsync(int userId)
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
                    }
                    catch
                    {
                        await uow.RollbackTransactionAsync();
                        throw;
                    }
                });

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all sessions for user {UserId}", userId);
                return Result<bool>.Failure("Error deleting all sessions.");
            }
        }

        /// <summary>
        /// Updates the title of a chat session for a user.
        /// </summary>
        public async Task<Result<bool>> UpdateSessionTitleAsync(string sessionId, int userId, string newTitle)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var strategy = uow.DbContext.Database.CreateExecutionStrategy();

                bool updated = false;

                await strategy.ExecuteAsync(async () =>
                {
                    await uow.BeginTransactionAsync();
                    try
                    {
                        var session = await uow.ChatRepository.GetSessionAsync(sessionId, userId);

                        if (session == null)
                        {
                            _logger.LogWarning("UpdateSessionTitleAsync: Session {SessionId} not found for user {UserId}", sessionId, userId);
                            await uow.RollbackTransactionAsync();
                            return;
                        }

                        session.Title = newTitle;
                        session.UpdatedAt = DateTime.UtcNow;

                        await uow.ChatRepository.UpdateSessionAsync(session);
                        await uow.SaveChangesAsync();
                        await uow.CommitTransactionAsync();

                        updated = true;
                    }
                    catch
                    {
                        await uow.RollbackTransactionAsync();
                        throw;
                    }
                });

                return updated ? Result<bool>.Success(true): Result<bool>.Failure("Session not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session title {SessionId}", sessionId);
                return Result<bool>.Failure("Error updating session title.");
            }
        }
    }
}
