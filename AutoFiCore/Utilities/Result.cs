using System.Collections.Generic;

namespace AutoFiCore.Utilities
{
    /// <summary>
    /// Represents the outcome of an operation, encapsulating success state, value, and error details.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// The value returned by a successful operation.
        /// </summary>
        public T? Value { get; private set; }

        /// <summary>
        /// A single error message describing the failure.
        /// </summary>
        public string? Error { get; private set; }

        /// <summary>
        /// A list of error messages describing the failure.
        /// </summary>
        public List<string> Errors { get; private set; } = new();

        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <param name="value">The result value.</param>
        /// <returns>A successful <see cref="Result{T}"/> instance.</returns>
        public static Result<T> Success(T value) => new()
        {
            IsSuccess = true,
            Value = value
        };

        /// <summary>
        /// Creates a failed result with a single error message.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
        public static Result<T> Failure(string error) => new()
        {
            IsSuccess = false,
            Error = error,
            Errors = new List<string> { error }
        };

        /// <summary>
        /// Creates a failed result with multiple error messages.
        /// </summary>
        /// <param name="errors">The list of error messages.</param>
        /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
        public static Result<T> Failure(List<string> errors) => new()
        {
            IsSuccess = false,
            Error = errors.Count > 0 ? errors[0] : null,
            Errors = errors
        };
    }
}