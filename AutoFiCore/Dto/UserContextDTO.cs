using AutoFiCore.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AutoFiCore.Dto
{
    public class UserContextDTO
    {
        [JsonPropertyName("auction_history")]
        public List<AuctionHistoryDTO> AuctionHistory { get; set; } = new();

        [JsonPropertyName("saved_searches")]
        public List<string> SavedSearches { get; set; } = new();

        [JsonPropertyName("auto_bid_settings")]
        public List<UserAutoBidSettings> AutoBidSettings { get; set; } = new();

        [JsonPropertyName("user_watchlist")]
        public List<WatchlistDTO> UserWatchlists { get; set; } = new();
    }
}
