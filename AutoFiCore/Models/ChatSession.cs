namespace AutoFiCore.Models
{
    public class ChatSession
    {
        public string Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ChatMessage> Messages { get; set; }
    }
}
