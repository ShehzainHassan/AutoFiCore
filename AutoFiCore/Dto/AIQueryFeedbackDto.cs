using System.Text.Json.Serialization;
using AutoFiCore.Enums;

namespace AutoFiCore.Dto
{
    public class AIQueryFeedbackDto
    {
        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }

        [JsonPropertyName("vote")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueryFeedback Vote { get; set; }
    }
}
