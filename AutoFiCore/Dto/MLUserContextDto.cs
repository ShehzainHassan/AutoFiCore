using System.Text.Json.Serialization;

public class UserInteractionDto
{
    [JsonPropertyName("VehicleId")]
    public int VehicleId { get; set; }

    [JsonPropertyName("InteractionType")]
    public string InteractionType { get; set; } = string.Empty;

    [JsonPropertyName("CreatedAt")]
    public string CreatedAt { get; set; } = string.Empty;
}

public class AnalyticsEventDto
{
    [JsonPropertyName("EventType")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("AuctionId")]
    public int AuctionId { get; set; }

    [JsonPropertyName("CreatedAt")]
    public string CreatedAt { get; set; } = string.Empty;
}

public class MLUserContextDto
{
    [JsonPropertyName("user_interactions")]
    public List<UserInteractionDto> UserInteractions { get; set; } = new();

    [JsonPropertyName("analytics_events")]
    public List<AnalyticsEventDto> AnalyticsEvents { get; set; } = new();
}
