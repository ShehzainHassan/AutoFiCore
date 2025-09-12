using AutoFiCore.Dto;
using AutoFiCore.Models;

namespace AutoFiCore.Data.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatSession?> GetSessionAsync(string sessionId, int userId);
        Task<List<ChatTitleDto>> GetUserChatsAsync(int userId);
        Task AddSessionAsync(ChatSession session);
        Task AddMessageAsync(ChatMessage message);
        Task UpdateSessionAsync(ChatSession session);
        Task DeleteSessionAsync(string sessionId, int userId);
        Task DeleteAllSessionsAsync(int userId);
        Task<string> SaveChatHistoryAsync(EnrichedAIQuery payload, AIResponseModel result);

    }
}
