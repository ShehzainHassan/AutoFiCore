using System;
using System.Text.Json.Serialization;

namespace AutoFiCore.Dto
{
    public class AuctionHistoryDTO
    {
        [JsonPropertyName("auction_id")]
        public int AuctionId { get; set; }

        [JsonPropertyName("vehicle_id")]
        public int VehicleId { get; set; }

        [JsonPropertyName("start_utc")]
        public DateTime StartUtc { get; set; }

        [JsonPropertyName("end_utc")]
        public DateTime EndUtc { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("current_price")]
        public decimal CurrentPrice { get; set; }

        [JsonPropertyName("reserve_price")]
        public decimal ReservePrice { get; set; }

        [JsonPropertyName("is_winner")]
        public bool IsWinner { get; set; }
    }
}
