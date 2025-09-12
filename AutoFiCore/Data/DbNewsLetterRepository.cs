using AutoFiCore.Data.Interfaces;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Data
{
    public class DbNewsLetterRepository : INewsLetterRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<DbNewsLetterRepository> _logger;
        public DbNewsLetterRepository(ApplicationDbContext dbContext, ILogger<DbNewsLetterRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<bool> IsAlreadySubscribed(string email)
        {
            var isExists = await _dbContext.Newsletters.AsNoTracking().AnyAsync(n => n.Email == email);
            if (isExists)
            {
                return true;
            }
            return false;
        }
        public Task<Newsletter> SubscribeToNewsletter(Newsletter newsletter)
        {
              _dbContext.Newsletters.Add(newsletter);
            return Task.FromResult(newsletter);
        }
    }
}
