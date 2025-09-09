using AutoFiCore.Data;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace AutoFiCore.Services
{
    public interface IRefreshTokenService
    {
        Task SaveAsync(int userId, string token);
        Task<RefreshToken?> GetAsync(string token);
        Task RotateAsync(RefreshToken oldToken, string newToken);
        Task<RefreshToken?> GetLatestTokenForUserAsync(int userId);
    }

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ApplicationDbContext _db;

        public RefreshTokenService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SaveAsync(int userId, string token)
        {
            var refresh = new RefreshToken
            {
                UserId = userId,
                Token = token,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            _db.RefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();
        }
        public async Task<RefreshToken?> GetAsync(string token)
        {
            return await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked);
        }
        public async Task RotateAsync(RefreshToken oldToken, string newToken)
        {
            oldToken.IsRevoked = true;
            var newRefresh = new RefreshToken
            {
                UserId = oldToken.UserId,
                Token = newToken,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            _db.RefreshTokens.Add(newRefresh);
            await _db.SaveChangesAsync();
        }
        public async Task<RefreshToken?> GetLatestTokenForUserAsync(int userId)
        {
            return await _db.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.Expires > DateTime.UtcNow)
                .OrderByDescending(rt => rt.Created)
                .FirstOrDefaultAsync();
        }
    }

}
