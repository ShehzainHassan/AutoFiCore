using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoFiCore.Services
{
    public interface IUserContextService
    {
        Task<UserContextDTO> GetUserContextAsync(int userId);
        Task<MLUserContextDto?> FetchMLContextAsync(int userId, string correlationId, string jwtToken);
    }

    public class UserContextService : IUserContextService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserContextService> _logger;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public UserContextService(
            IUnitOfWork uow,
            IHttpClientFactory httpClientFactory,
            ILogger<UserContextService> logger,
            IMemoryCache memoryCache)
        {
            _uow = uow;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _cache = memoryCache;
        }

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
                                auction.CurrentPrice >= auction.ReservePrice &&
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
        /// Uses in-memory caching to avoid repeated database calls for the same user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The <see cref="UserContextDTO"/> for the given user.</returns>
        public async Task<UserContextDTO> GetUserContextAsync(int userId)
        {
            var context = await _cache.GetOrCreateAsync(userId, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;

                _logger.LogInformation("Cache miss for user {UserId}. Fetching context...", userId);

                return new UserContextDTO
                {
                    SavedSearches = await _uow.Users.GetUserSavedSearches(userId),
                    AuctionHistory = await GetUserAuctionHistoryAsync(userId),
                    AutoBidSettings = await _uow.AutoBid.GetUserAutoBidSettingsAsync(userId)
                };
            });

            return context!;
        }


        /// <summary>
        /// Fetches the ML context for the user from the FastAPI service.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="correlationId">Correlation ID for logging/tracing.</param>
        /// <param name="jwtToken">JWT authentication token.</param>
        /// <returns>An <see cref="MLUserContextDto"/> if successful, otherwise null.</returns>
        public async Task<MLUserContextDto?> FetchMLContextAsync(int userId, string correlationId, string jwtToken)
        {
            var client = _httpClientFactory.CreateClient("FastApi");
            client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var requestUri = $"context/{userId}";
            _logger.LogInformation("Initiating ML context fetch. CorrelationId={CorrelationId}, RequestUri={RequestUri}", correlationId, requestUri);

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(requestUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP request to FastAPI failed. CorrelationId={CorrelationId}", correlationId);
                return null;
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Received response. CorrelationId={CorrelationId}, StatusCode={StatusCode}, RawJson={RawJson}", correlationId, response.StatusCode, resultJson);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch ML context. CorrelationId={CorrelationId}, StatusCode={StatusCode}, ResponseBody={ResponseBody}", correlationId, response.StatusCode, resultJson);
                return null;
            }

            try
            {
                var context = JsonSerializer.Deserialize<MLUserContextDto>(resultJson, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                if (context == null)
                {
                    _logger.LogWarning("Deserialized ML context is null. CorrelationId={CorrelationId}", correlationId);
                    return null;
                }

                _logger.LogInformation("Deserialization successful. CorrelationId={CorrelationId}, UserInteractions={UserInteractionsCount}, AnalyticsEvents={AnalyticsEventsCount}",
                    correlationId, context.UserInteractions.Count, context.AnalyticsEvents.Count);

                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deserialization failed for ML context. CorrelationId={CorrelationId}, RawJson={RawJson}", correlationId, resultJson);
                return null;
            }
        }
    }
}
