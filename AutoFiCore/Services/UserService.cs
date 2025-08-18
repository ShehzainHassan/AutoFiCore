using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using AutoFiCore.Utilities;

namespace AutoFiCore.Services
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

    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserService> _logger;
        private readonly TokenProvider _tokenProvider;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepository repository, ILogger<UserService> logger, TokenProvider tokenProvider, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _logger = logger;
            _tokenProvider = tokenProvider;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<User>> AddUserAsync(User user)
        {
            var existingUser = await _repository.IsEmailExists(user.Email);
            if (existingUser)
                return Result<User>.Failure("User already exists");

            var createdUser = await _unitOfWork.Users.AddUserAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return Result<User>.Success(createdUser);
        }
        public async Task<AuthResponse?> LoginUserAsync(string email, string password)
        {
            return await _repository.LoginUserAsync(email, password, _tokenProvider);
        }
        public async Task<UserLikes> AddUserLikeAsync(UserLikes userLikes)
        {
            await _unitOfWork.Users.AddUserLikeAsync(userLikes);
            await _unitOfWork.SaveChangesAsync();
            return userLikes;
        }
        public async Task<UserLikes?> RemoveUserLikeAsync(UserLikes userLikes)
        {
            await _unitOfWork.Users.RemoveUserLikeAsync(userLikes);
            await _unitOfWork.SaveChangesAsync();
            return userLikes;
        }
        public async Task<UserSavedSearch?> RemoveSavedSearchAsync(UserSavedSearch savedSearch)
        {
            await _unitOfWork.Users.RemoveUserSearchAsync(savedSearch);
            await _unitOfWork.SaveChangesAsync();
            return savedSearch;
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _repository.GetUserByIdAsync(id);
        }
        public async Task<UserSavedSearch> AddUserSearchAsync(UserSavedSearch search)
        {
            await _unitOfWork.Users.AddUserSearchAsync(search);
            await _unitOfWork.SaveChangesAsync();
            return search;
        }
        public async Task<List<string>> GetUserLikedVinsAsync(int id)
        {
            return await _repository.GetUserLikesVehicles(id);
        }
        public async Task<List<string>> GetUserSavedSearches(int id)
        {
            return await _repository.GetUserSavedSearches(id);
        }
        public async Task<UserInteractions> AddUserInteractionAsync(UserInteractions userInteractions)
        {
            await _unitOfWork.Users.AddUserInteraction(userInteractions);
            await _unitOfWork.SaveChangesAsync();
            return userInteractions;
        }

        public async Task<int> GetAllUsersCountAsync()
        {
            return await _unitOfWork.Users.GetAllUsersCountAsync();
        }
        public async Task<DateTime?> GetOldestUserCreatedDateAsync()
        {
            return await _unitOfWork.Users.GetOldestUserCreatedDateAsync();

        }

    }
}
