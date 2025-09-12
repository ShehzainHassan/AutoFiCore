using AutoFiCore.Dto;

namespace AutoFiCore.Data.Interfaces
{
    public interface IAIAssistantService
    {
        Task<AIResponseModel> QueryFastApiAsync(EnrichedAIQuery payload, string correlationId, string jwtToken);
        Task<List<ChatTitleDto>> GetChatTitlesAsync(int userId);
        Task<ChatSessionDto?> GetFullChatAsync(int userId, string sessionId);
        Task<FeedbackResponseDto> SubmitFeedbackAsync(AIQueryFeedbackDto feedbackDto, string correlationId, string jwtToken);
        Task<List<string>> GetSuggestionsAsync(int userId, string correlationId, string jwtToken);
        Task<List<PopularQueryDto>> GetPopularQueriesAsync(int limit, string correlationId, string jwtToken);
        Task DeleteSessionAsync(string sessionId, int userId);
        Task DeleteAllSessionsAsync(int userId);
        Task UpdateSessionTitleAsync(string sessionId, int userId, string newTitle);
    }
}
