using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoFiCore.Data
{
    public interface IChatRepository
    {
        Task<ChatSession?> GetSessionAsync(string sessionId, int userId);
        Task<List<ChatTitleDto>> GetUserChatsAsync(int userId);
        Task AddSessionAsync(ChatSession session);
        Task AddMessageAsync(ChatMessage message);
        Task SaveChangesAsync();
        Task UpdateSessionAsync(ChatSession session);
        Task DeleteSessionAsync(string sessionId, int userId);
        Task DeleteAllSessionsAsync(int userId);

    }

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
        public Task SaveChangesAsync() => _db.SaveChangesAsync();
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
            await _db.SaveChangesAsync();
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
                await _db.SaveChangesAsync();
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
                await _db.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation($"DeleteAllSessionsAsync: No sessions found for user {userId}.");
            }
        }
    
    }
}
