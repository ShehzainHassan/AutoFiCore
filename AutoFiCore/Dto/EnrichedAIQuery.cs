using AutoFiCore.Dto;
using System.Text.Json.Serialization;

public class EnrichedAIQuery
{
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("query")]
    public AIQueryRequest Query { get; set; } = new();

    [JsonPropertyName("context")]
    public UserContextDTO Context { get; set; } = new();

    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }
}


