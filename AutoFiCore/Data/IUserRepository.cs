using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace AutoFiCore.Data
{
    public interface IUserRepository
    {
        Task<int> GetUserCountAsync(DateTime start, DateTime end);
        Task<User> AddUserAsync(User user);
        Task<AuthResponse?> LoginUserAsync(string email, string password, TokenProvider tokenProvider);
        Task<UserLikes> AddUserLikeAsync(UserLikes userlikes);
        Task<User?> GetUserByIdAsync(int id);
        Task<List<string>> GetUserLikesVehicles(int id);
        Task<UserLikes?> RemoveUserLikeAsync(UserLikes userLikes);
        Task<UserSavedSearch> AddUserSearchAsync(UserSavedSearch search);
        Task<UserSavedSearch?> RemoveUserSearchAsync(UserSavedSearch search);
        Task<List<string>> GetUserSavedSearches(int id);
        Task<UserInteractions> AddUserInteraction(UserInteractions userInteractions);
        Task<bool> IsEmailExists(string email);
        Task<int> GetAllUsersCountAsync();
    }
}
