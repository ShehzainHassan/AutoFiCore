using AutoFiCore.Enums;

namespace AutoFiCore.Dto
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string Sender { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public QueryFeedback Feedback { get; set; }
        public string? UiType { get; set; }
        public string? QueryType { get; set; }
        public List<string>? SuggestedActions { get; set; } = new();
        public List<string>? Sources { get; set; } = new();
    }
}
