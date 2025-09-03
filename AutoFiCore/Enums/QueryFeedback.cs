using System.Text.Json.Serialization;

namespace AutoFiCore.Enums
{
    public enum QueryFeedback
    {
        [JsonPropertyName("NOTVOTED")]
        NotVoted = 0,

        [JsonPropertyName("UPVOTED")]
        Upvoted = 1,

        [JsonPropertyName("DOWNVOTED")]
        Downvoted = 2,
    }
}
