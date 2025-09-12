using AutoFiCore.Dto;
using AutoFiCore.Models;

namespace AutoFiCore.Utilities
{
    public static class ChatMapper
    {
        public static ChatSessionDto ToDto(ChatSession session) => new ChatSessionDto
        {
            Id = session.Id,
            Title = session.Title,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            Messages = session.Messages
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Sender = m.Sender,
                    Message = m.Message,
                    Timestamp = m.Timestamp,
                    Feedback = m.Feedback,
                    UiType = m.UiType,
                    QueryType = m.QueryType,
                    SuggestedActions = m.SuggestedActions ?? new List<string>(),
                    Sources = m.Sources ?? new List<string>(),
                })
                .OrderBy(m => m.Id)
                .ToList()
        };
    }
}
