using AutoFiCore.Enums;
using System.Text.Json.Serialization;

namespace AutoFiCore.Dto
{
    public class FeedbackResponseDto
    {
        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }

        [JsonPropertyName("feedback")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueryFeedback Feedback { get; set; }
    }
}
