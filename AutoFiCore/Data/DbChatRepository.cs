using AutoFiCore.Data.Interfaces;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoFiCore.Data
{

    public class DbChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DbChatRepository> _logger;
        public DbChatRepository(ApplicationDbContext db, ILogger<DbChatRepository> logger)
        {
            _db = db;
            _logger = logger;
        }
        public Task<ChatSession?> GetSessionAsync(string sessionId, int userId) =>
            _db.ChatSessions.Include(s => s.Messages).FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        public Task AddSessionAsync(ChatSession session)
        {
            _db.ChatSessions.Add(session);
            return Task.CompletedTask;
        }
        public Task AddMessageAsync(ChatMessage message)
        {
            _db.ChatMessages.Add(message);
            return Task.CompletedTask;
        }
        public Task<List<ChatTitleDto>> GetUserChatsAsync(int userId) =>
            _db.ChatSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .Select(s => new ChatTitleDto
            {
                  Id = s.Id,
                  Title = s.Title,
                  CreatedAt = s.CreatedAt,
                  UpdatedAt = s.UpdatedAt
            }).ToListAsync();
        public async Task UpdateSessionAsync(ChatSession session)
        {
            _db.ChatSessions.Update(session);
        }
        public async Task DeleteSessionAsync(string sessionId, int userId)
        {
            var session = await _db.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

            if (session != null)
            {
                _db.ChatMessages.RemoveRange(session.Messages);
                _db.ChatSessions.Remove(session);
            }
            else
            {
                _logger.LogWarning($"DeleteSessionAsync: Session {sessionId} not found for user {userId}.");
            }
        }
        public async Task DeleteAllSessionsAsync(int userId)
        {
            var sessions = await _db.ChatSessions
                .Where(s => s.UserId == userId)
                .Include(s => s.Messages)
                .ToListAsync();

            if (sessions.Any())
            {
                var allMessages = sessions.SelectMany(s => s.Messages);
                _db.ChatMessages.RemoveRange(allMessages);
                _db.ChatSessions.RemoveRange(sessions);
            }
            else
            {
                _logger.LogInformation($"DeleteAllSessionsAsync: No sessions found for user {userId}.");
            }
        }
        public async Task<string> SaveChatHistoryAsync(EnrichedAIQuery payload, AIResponseModel result)
        {
            if (string.IsNullOrWhiteSpace(payload.SessionId))
            {
                payload.SessionId = Guid.NewGuid().ToString();
            }

            var session = await GetSessionAsync(payload.SessionId, payload.UserId);

            if (session == null)
            {
                string GenerateTitle(string question)
                {
                    if (string.IsNullOrWhiteSpace(question)) return "New Chat";
                    var title = question.Length > 50 ? question[..50] : question;
                    return title.Replace("\r", " ").Replace("\n", " ").Trim();
                }

                session = new ChatSession
                {
                    Id = payload.SessionId,
                    UserId = payload.UserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Title = GenerateTitle(payload.Query.Question),
                    Messages = new List<ChatMessage>()
                };

                await AddSessionAsync(session);
            }
            else
            {
                session.UpdatedAt = DateTime.UtcNow;
                _db.ChatSessions.Update(session);
            }

            var userMessage = new ChatMessage
            {
                ChatSessionId = session.Id,
                Sender = "User",
                Message = payload.Query.Question,
                Timestamp = DateTime.UtcNow
            };
            await AddMessageAsync(userMessage);

            var aiMessage = new ChatMessage
            {
                ChatSessionId = session.Id,
                Sender = "AI",
                Message = result.UiBlock,
                Timestamp = DateTime.UtcNow,
                UiType = result.UiType,
                QueryType = result.QueryType,
                SuggestedActions = result.SuggestedActions,
                Sources = result.Sources,
            };
            await AddMessageAsync(aiMessage);

            return session.Id;
        }
    }
}
