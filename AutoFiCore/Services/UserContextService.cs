using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoFiCore.Services
{

    /// <summary>
    /// Default implementation of <see cref="IUserContextService"/> which uses a UnitOfWork for DB access
    /// and <see cref="IDistributedCache"/> for Redis-backed caching.
    /// </summary>
    public class UserContextService : IUserContextService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserContextService> _logger;
        private readonly IDistributedCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
        private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Creates a new instance of <see cref="UserContextService"/>.
        /// </summary>
        /// <param name="uow">Unit of work for repository access.</param>
        /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="cache">Distributed cache (Redis).</param>
        public UserContextService(IUnitOfWork uow, IHttpClientFactory httpClientFactory, ILogger<UserContextService> logger, IDistributedCache cache)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Build auction history DTO list from user bids.
        /// </summary>
        /// <param name="userId">User id</param>
        private async Task<List<AuctionHistoryDTO>> GetUserAuctionHistoryAsync(int userId)
        {
            var userBids = await _uow.Bids.GetBidsByUserIdAsync(userId);
            var auctionIds = userBids.Select(b => b.AuctionId).Distinct().ToList();

            var auctionHistory = new List<AuctionHistoryDTO>();

            foreach (var auctionId in auctionIds)
            {
                var auction = await _uow.Auctions.GetAuctionByIdAsync(auctionId);
                if (auction == null) continue;

                var highestBidderId = await _uow.Bids.GetHighestBidderIdAsync(auctionId);

                bool isWinner = auction.Status == AuctionStatus.Ended &&
                                auction.CurrentPrice >= (auction.ReservePrice ?? auction.StartingPrice) &&
                                highestBidderId.HasValue &&
                                highestBidderId.Value == userId;

                auctionHistory.Add(new AuctionHistoryDTO
                {
                    AuctionId = auction.AuctionId,
                    VehicleId = auction.VehicleId,
                    StartUtc = auction.StartUtc,
                    EndUtc = auction.EndUtc,
                    CurrentPrice = auction.CurrentPrice,
                    Status = auction.Status.ToString(),
                    ReservePrice = auction.ReservePrice ?? auction.StartingPrice,
                    IsWinner = isWinner
                });
            }

            return auctionHistory;
        }

        /// <summary>
        /// Retrieves the user context including saved searches, auction history, and auto-bid settings.
        /// </summary>
        /// <param name="userId">The id of the user to fetch context for.</param>

        public async Task<Result<UserContextDTO>> GetUserContextAsync(int userId)
        {
            try
            {
                var cacheKey = $"user_context:{userId}";
                var cachedData = await _cache.GetStringAsync(cacheKey).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Cache hit for user {UserId}", userId);
                    var cachedContext = JsonSerializer.Deserialize<UserContextDTO>(cachedData, _serializerOptions);
                    if (cachedContext != null)
                    {
                        return Result<UserContextDTO>.Success(cachedContext);
                    }

                    _logger.LogWarning("Cached user context deserialization returned null for user {UserId}. Recomputing.", userId);
                }

                _logger.LogInformation("Cache miss for user {UserId}. Fetching context...", userId);

                var context = new UserContextDTO
                {
                    SavedSearches = await _uow.Users.GetUserSavedSearches(userId).ConfigureAwait(false),
                    AuctionHistory = await GetUserAuctionHistoryAsync(userId).ConfigureAwait(false),
                    AutoBidSettings = await _uow.AutoBid.GetUserAutoBidSettingsAsync(userId).ConfigureAwait(false)
                };

                var serialized = JsonSerializer.Serialize(context, _serializerOptions);
                await _cache.SetStringAsync(
                    cacheKey,
                    serialized,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheExpiration
                    }).ConfigureAwait(false);

                return Result<UserContextDTO>.Success(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user context for UserId={UserId}", userId);
                return Result<UserContextDTO>.Failure("Failed to load user context.");
            }
        }

        /// <summary>
        /// Fetches the ML context for the user from the ML/FastAPI service.
        /// Returns a Result containing an MLUserContextDto or null (when ML service returns nothing).
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="correlationId">Correlation id used for tracing/logging</param>
        /// <param name="jwtToken">JWT token used to authorize the downstream request</param>

        public async Task<Result<MLUserContextDto?>> FetchMLContextAsync(int userId, string correlationId, string jwtToken)
        {
            var client = _httpClientFactory.CreateClient("FastApi");
            if (!string.IsNullOrWhiteSpace(correlationId))
                client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

            if (!string.IsNullOrWhiteSpace(jwtToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var requestUri = $"context/{userId}";
            _logger.LogInformation("Initiating ML context fetch. CorrelationId={CorrelationId}, RequestUri={RequestUri}", correlationId, requestUri);

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(requestUri).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP request to FastAPI failed. CorrelationId={CorrelationId}", correlationId);
                return Result<MLUserContextDto?>.Failure("Failed to reach ML service.");
            }

            var resultJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _logger.LogInformation("Received response. CorrelationId={CorrelationId}, StatusCode={StatusCode}", correlationId, response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch ML context. CorrelationId={CorrelationId}, StatusCode={StatusCode}, ResponseBody={ResponseBody}",
                    correlationId, response.StatusCode, resultJson);
                return Result<MLUserContextDto?>.Failure($"FastAPI returned {response.StatusCode}");
            }

            try
            {
                var context = JsonSerializer.Deserialize<MLUserContextDto?>(resultJson, _serializerOptions);

                if (context == null)
                {
                    _logger.LogInformation("ML context endpoint returned empty payload. CorrelationId={CorrelationId}", correlationId);
                    return Result<MLUserContextDto?>.Success(null);
                }

                _logger.LogInformation("Deserialization successful. CorrelationId={CorrelationId}, UserInteractions={UserInteractionsCount}, AnalyticsEvents={AnalyticsEventsCount}",
                    correlationId, context.UserInteractions?.Count ?? 0, context.AnalyticsEvents?.Count ?? 0);

                return Result<MLUserContextDto?>.Success(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deserialization failed for ML context. CorrelationId={CorrelationId}, RawJsonLength={Length}", correlationId, resultJson?.Length ?? 0);
                return Result<MLUserContextDto?>.Failure("Failed to deserialize ML context.");
            }
        }
    }
}
