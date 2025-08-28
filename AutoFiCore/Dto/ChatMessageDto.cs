namespace AutoFiCore.Dto
{
    public class ChatMessageDto
    {
        public string Sender { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
