using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoFiCore.Data.Interfaces
{
    /// <summary>
    /// Defines user-related operations including authentication, preferences, interactions, and analytics.
    /// All methods return a <see cref="Result{T}"/> to encapsulate success state and error details.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Adds a new user to the system if the email is not already registered.
        /// </summary>
        /// <param name="user">The user entity to be added.</param>
        /// <returns>A result containing the created user or an error message.</returns>
        Task<Result<User>> AddUserAsync(User user);

        /// <summary>
        /// Authenticates a user using email and password, and returns an authentication response with tokens.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>A result containing the authentication response or an error message.</returns>
        Task<Result<AuthResponse>> LoginUserAsync(string email, string password);

        /// <summary>
        /// Adds a vehicle like for the specified user.
        /// </summary>
        /// <param name="userLikes">The user like entity containing user and VIN details.</param>
        /// <returns>A result containing the added like or an error message.</returns>
        Task<Result<UserLikes>> AddUserLikeAsync(UserLikes userLikes);

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The user's ID.</param>
        /// <returns>A result containing the user entity or an error message.</returns>
        Task<Result<User>> GetUserByIdAsync(int id);

        /// <summary>
        /// Retrieves a list of VINs liked by the specified user.
        /// </summary>
        /// <param name="id">The user's ID.</param>
        /// <returns>A result containing the list of liked VINs or an error message.</returns>
        Task<Result<List<string>>> GetUserLikedVinsAsync(int id);

        /// <summary>
        /// Removes a previously liked vehicle for the specified user.
        /// </summary>
        /// <param name="userLikes">The user like entity to be removed.</param>
        /// <returns>A result containing the removed like or an error message.</returns>
        Task<Result<UserLikes>> RemoveUserLikeAsync(UserLikes userLikes);

        /// <summary>
        /// Adds a saved search for the specified user.
        /// </summary>
        /// <param name="search">The saved search entity to be added.</param>
        /// <returns>A result containing the added search or an error message.</returns>
        Task<Result<UserSavedSearch>> AddUserSearchAsync(UserSavedSearch search);

        /// <summary>
        /// Removes a saved search for the specified user.
        /// </summary>
        /// <param name="savedSearch">The saved search entity to be removed.</param>
        /// <returns>A result containing the removed search or an error message.</returns>
        Task<Result<UserSavedSearch>> RemoveSavedSearchAsync(UserSavedSearch savedSearch);

        /// <summary>
        /// Retrieves all saved search for the specified user.
        /// </summary>
        /// <param name="id">The user's ID.</param>
        /// <returns>A result containing the list of saved searches or an error message.</returns>
        Task<Result<List<string>>> GetUserSavedSearchesAsync(int id);

        /// <summary>
        /// Adds a user interaction event (e.g., clicks, views, etc.) for analytics or personalization.
        /// </summary>
        /// <param name="userInteractions">The interaction entity to be recorded.</param>
        /// <returns>A result containing the added interaction or an error message.</returns>
        Task<Result<UserInteractions>> AddUserInteractionAsync(UserInteractions userInteractions);

        /// <summary>
        /// Retrieves the total number of users in the system.
        /// </summary>
        /// <returns>A result containing the user count or an error message.</returns>
        Task<Result<int>> GetAllUsersCountAsync();

        /// <summary>
        /// Retrieves the creation date of the oldest registered user.
        /// </summary>
        /// <returns>A result containing the oldest creation date or an error message.</returns>
        Task<Result<DateTime?>> GetOldestUserCreatedDateAsync();
    }
}