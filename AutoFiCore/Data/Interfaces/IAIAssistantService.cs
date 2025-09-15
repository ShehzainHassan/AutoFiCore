using AutoFiCore.Dto;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IAIAssistantService
    {
        Task<Result<AIResponseModel>> QueryFastApiAsync(EnrichedAIQuery payload, string correlationId, string jwtToken);
        Task<Result<List<ChatTitleDto>>> GetChatTitlesAsync(int userId);
        Task<Result<ChatSessionDto?>> GetFullChatAsync(int userId, string sessionId);
        Task<Result<FeedbackResponseDto>> SubmitFeedbackAsync(AIQueryFeedbackDto feedbackDto, string correlationId, string jwtToken);
        Task<Result<List<string>>> GetSuggestionsAsync(int userId, string correlationId, string jwtToken);
        Task<Result<List<PopularQueryDto>>> GetPopularQueriesAsync(int limit, string correlationId, string jwtToken);
        Task<Result<bool>> DeleteSessionAsync(string sessionId, int userId);
        Task<Result<bool>> DeleteAllSessionsAsync(int userId);
        Task<Result<bool>> UpdateSessionTitleAsync(string sessionId, int userId, string newTitle);
    }
}
