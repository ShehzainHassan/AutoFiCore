using System;
using System.Text.Json.Serialization;

namespace AutoFiCore.Dto
{
    public class PopularQueryDto
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("last_asked")]
        public DateTime? LastAsked { get; set; }
    }
}
