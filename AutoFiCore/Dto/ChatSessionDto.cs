namespace AutoFiCore.Dto
{
    public class ChatSessionDto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new();
    }
}
