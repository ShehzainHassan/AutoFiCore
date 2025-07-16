using System.Text.Json.Serialization;

namespace AutoFiCore.Dto
{
    public class AuctionQueryParams
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AuctionStatus? Status { get; set; }
        public string? Make { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; } = false;
    }
}
