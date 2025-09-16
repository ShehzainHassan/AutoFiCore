using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using AutoFiCore.Utilities;
using AutoFiCore.Data.Interfaces;

namespace AutoFiCore.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserService> _logger;
        private readonly ITokenProvider _tokenProvider;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepository repository, ILogger<UserService> logger, ITokenProvider tokenProvider, IUnitOfWork unitOfWork, IRefreshTokenService refreshTokenService)
        {
            _repository = repository;
            _logger = logger;
            _tokenProvider = tokenProvider;
            _unitOfWork = unitOfWork;
            _refreshTokenService = refreshTokenService;
        }

        /// <inheritdoc />
        public async Task<Result<User>> AddUserAsync(User user)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var existingUser = await _repository.IsEmailExists(user.Email);
                    if (existingUser)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<User>.Failure("User already exists");
                    }

                    var createdUser = await _unitOfWork.Users.AddUserAsync(user);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<User>.Success(createdUser);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Failed to add user. Email={Email}", user.Email);
                    return Result<User>.Failure("Failed to add user.");
                }
            });
        }

        /// <inheritdoc />
        public async Task<Result<AuthResponse>> LoginUserAsync(string email, string password)
        {
            try
            {
                var response = await _repository.LoginUserAsync(email, password, _tokenProvider, _refreshTokenService);

                if (response == null)
                    return Result<AuthResponse>.Failure("Invalid email or password.");

                return Result<AuthResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for Email={Email}", email);
                return Result<AuthResponse>.Failure("An error occurred while logging in.");
            }
        }

        /// <inheritdoc />
        public async Task<Result<UserLikes>> AddUserLikeAsync(UserLikes userLikes)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    await _unitOfWork.Users.AddUserLikeAsync(userLikes);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<UserLikes>.Success(userLikes);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Failed to add user like. UserId={UserId}", userLikes.userId);
                    return Result<UserLikes>.Failure("Failed to add user like.");
                }
            });
        }

        /// <inheritdoc />
        public async Task<Result<UserLikes>> RemoveUserLikeAsync(UserLikes userLikes)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    await _unitOfWork.Users.RemoveUserLikeAsync(userLikes);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<UserLikes>.Success(userLikes);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Failed to remove user like. UserId={UserId}", userLikes.userId);
                    return Result<UserLikes>.Failure("Failed to remove user like.");
                }
            });
        }

        /// <inheritdoc />
        public async Task<Result<UserSavedSearch>> RemoveSavedSearchAsync(UserSavedSearch savedSearch)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    await _unitOfWork.Users.RemoveUserSearchAsync(savedSearch);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<UserSavedSearch>.Success(savedSearch);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Failed to remove saved search. UserId={UserId}", savedSearch.userId);
                    return Result<UserSavedSearch>.Failure("Failed to remove saved search.");
                }
            });
        }

        /// <inheritdoc />
        public async Task<Result<User>> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _repository.GetUserByIdAsync(id);
                return user != null
                    ? Result<User>.Success(user)
                    : Result<User>.Failure("User not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user. Id={Id}", id);
                return Result<User>.Failure("Error retrieving user.");
            }
        }

        /// <inheritdoc />
        public async Task<Result<UserSavedSearch>> AddUserSearchAsync(UserSavedSearch search)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    await _unitOfWork.Users.AddUserSearchAsync(search);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<UserSavedSearch>.Success(search);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Failed to add saved search. UserId={UserId}", search.userId);
                    return Result<UserSavedSearch>.Failure("Failed to add saved search.");
                }
            });
        }

        /// <inheritdoc />
        public async Task<Result<List<string>>> GetUserLikedVinsAsync(int id)
        {
            try
            {
                var vins = await _repository.GetUserLikesVehicles(id);
                return Result<List<string>>.Success(vins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve liked VINs. UserId={UserId}", id);
                return Result<List<string>>.Failure("Error retrieving liked VINs.");
            }
        }

        /// <inheritdoc />
        public async Task<Result<List<string>>> GetUserSavedSearchesAsync(int id)
        {
            try
            {
                var searches = await _repository.GetUserSavedSearches(id);
                return Result<List<string>>.Success(searches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve saved searches. UserId={UserId}", id);
                return Result<List<string>>.Failure("Error retrieving saved searches.");
            }
        }

        /// <inheritdoc />
        public async Task<Result<UserInteractions>> AddUserInteractionAsync(UserInteractions userInteractions)
        {
            var strategy = _unitOfWork.DbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    await _unitOfWork.Users.AddUserInteraction(userInteractions);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Result<UserInteractions>.Success(userInteractions);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Failed to add user interaction. UserId={UserId}", userInteractions.UserId);
                    return Result<UserInteractions>.Failure("Failed to add user interaction.");
                }
            });
        }

        /// <inheritdoc />
        public async Task<Result<int>> GetAllUsersCountAsync()
        {
            try
            {
                var count = await _unitOfWork.Users.GetAllUsersCountAsync();
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user count.");
                return Result<int>.Failure("Error retrieving user count.");
            }
        }

        /// <inheritdoc />
        public async Task<Result<DateTime?>> GetOldestUserCreatedDateAsync()
        {
            try
            {
                var date = await _unitOfWork.Users.GetOldestUserCreatedDateAsync();
                return Result<DateTime?>.Success(date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve oldest user creation date.");
                return Result<DateTime?>.Failure("Error retrieving oldest user creation date.");
            }
        }
    }
}
