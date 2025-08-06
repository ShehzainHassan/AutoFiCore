using AutoFiCore.Dto;
using AutoFiCore.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Polly;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace AutoFiCore.Data
{
    public class DbUserRepository:IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<DbUserRepository> _logger;
        public DbUserRepository(ApplicationDbContext dbContext, ILogger<DbUserRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public Task<int> GetAllUsersCountAsync()
        {
            return _dbContext.Users.CountAsync();
        }
        public Task<int> GetUserCountAsync(DateTime start, DateTime end) => _dbContext.Users.CountAsync();
        public async Task<bool> IsEmailExists(string email)
        {
            var isExists = await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Email == email);
            if (isExists)
            {
                return true;
            }
            return false;
        }
        public Task<User> AddUserAsync(User user)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _dbContext.Users.Add(user);
            return Task.FromResult(user);
        }
        public Task<UserInteractions> AddUserInteraction(UserInteractions userInteractions)
        {
            _dbContext.UserInteractions.Add(userInteractions);
            return Task.FromResult(userInteractions);
        }
        public Task<UserSavedSearch> AddUserSearchAsync(UserSavedSearch search)
        {
            _dbContext.UserSavedSearches.Add(search);
            return Task.FromResult(search);
        }
        public async Task<UserSavedSearch?> RemoveUserSearchAsync(UserSavedSearch search)
        {
            var savedSearch = await _dbContext.UserSavedSearches.FirstOrDefaultAsync(us => us.userId == search.userId && us.search == search.search);
            if (savedSearch == null)
            {
                return null;
            }
            _dbContext.UserSavedSearches.Remove(savedSearch);
            return search;
        }
        public async Task<List<string>> GetUserLikesVehicles(int id)
        {
            return await _dbContext.UserLikes
                .AsNoTracking()
                .Where(ul => ul.userId == id)
                .Select(ul => ul.vehicleVin)
                .ToListAsync();
        }
        public async Task<List<string>> GetUserSavedSearches(int id)
        {
            return await _dbContext.UserSavedSearches
                .AsNoTracking()
                .Where(us => us.userId == id)
                .Select(us => us.search)
                .ToListAsync();
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _dbContext.Users.FindAsync(id);
        }
        public Task<UserLikes> AddUserLikeAsync(UserLikes userlikes)
        {
            _dbContext.UserLikes.Add(userlikes);
            return Task.FromResult(userlikes);
        }
        public async Task<UserLikes?> RemoveUserLikeAsync(UserLikes userLikes)
        {
            var like = await _dbContext.UserLikes
                .FirstOrDefaultAsync(ul => ul.userId == userLikes.userId && ul.vehicleVin == userLikes.vehicleVin);

            if (like == null)
            {
                return null;
            }

            _dbContext.UserLikes.Remove(like);
            return like;
        }
        public async Task<AuthResponse?> LoginUserAsync(string email, string password, TokenProvider tokenProvider)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return null;  
            }
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return null;
            }
            user.LastLoggedIn = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            string token = tokenProvider.Create(user);
            return new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                UserName = user.Name,
                UserEmail = user.Email
            };           
        }
    }
}

