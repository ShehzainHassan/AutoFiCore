using System.Text.Json.Serialization;

namespace AutoFiCore.Dto
{
    /// <summary>
    /// Represents the response returned by the AI Assistant.
    /// </summary>
    public class AIResponseModel
    {
        [JsonPropertyName("answer")]
        public string Answer { get; set; }

        [JsonPropertyName("ui_type")]
        public string UiType { get; set; }

        [JsonPropertyName("query_type")]
        public string QueryType { get; set; }

        [JsonPropertyName("data")]
        public object Data { get; set; }

        [JsonPropertyName("suggested_actions")]
        public List<string> SuggestedActions { get; set; }

        [JsonPropertyName("sources")]
        public List<string> Sources { get; set; }

        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }

        [JsonPropertyName("ui_block")]
        public string UiBlock { get; set; }
    }
}