using AutoFiCore.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public string ChatSessionId { get; set; }
    public ChatSession ChatSession { get; set; }
    public string Sender { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UiType { get; set; }
    public string? QueryType { get; set; }
    public List<string>? SuggestedActions { get; set; } = new();
    public List<string>? Sources { get; set; } = new();
}