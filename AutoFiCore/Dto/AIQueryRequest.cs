using System.Text.Json.Serialization;

namespace AutoFiCore.Dto
{
    public class AIQueryRequest
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }
    }
}

