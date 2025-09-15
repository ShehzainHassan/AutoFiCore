using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IUserService
    {
        Task<Result<User>> AddUserAsync(User user);
        Task<AuthResponse?> LoginUserAsync(string email, string password);
        Task<UserLikes> AddUserLikeAsync(UserLikes userlikes);
        Task<User?> GetUserByIdAsync(int id);
        Task<List<string>> GetUserLikedVinsAsync(int id);
        Task<UserLikes?> RemoveUserLikeAsync(UserLikes userLikes);
        Task<UserSavedSearch> AddUserSearchAsync(UserSavedSearch search);
        Task<UserSavedSearch?> RemoveSavedSearchAsync(UserSavedSearch savedSearch);
        Task<List<string>> GetUserSavedSearches(int id);
        Task<UserInteractions> AddUserInteractionAsync(UserInteractions userInteractions);
        Task<int> GetAllUsersCountAsync();
        Task<DateTime?> GetOldestUserCreatedDateAsync();
    }
}
