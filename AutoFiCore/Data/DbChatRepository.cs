using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoFiCore.Data
{
    public interface IChatRepository
    {
        Task<ChatSession?> GetSessionAsync(string sessionId, int userId);
        Task<List<ChatSession>> GetUserChatsAsync(int userId);
        Task AddSessionAsync(ChatSession session);
        Task AddMessageAsync(ChatMessage message);
        Task SaveChangesAsync();
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
        public Task<List<ChatSession>> GetUserChatsAsync(int userId) =>
           _db.ChatSessions
              .Where(s => s.UserId == userId)
              .OrderByDescending(s => s.CreatedAt)
              .ToListAsync();
    }
}
