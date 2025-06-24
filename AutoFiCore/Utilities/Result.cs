using System.Collections.Generic;

namespace AutoFiCore.Utilities
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Value { get; private set; }
        public string? Error { get; private set; }
        public List<string> Errors { get; private set; } = new();

        public static Result<T> Success(T value) => new()
        {
            IsSuccess = true,
            Value = value
        };

        public static Result<T> Failure(string error) => new()
        {
            IsSuccess = false,
            Error = error
        };

        public static Result<T> Failure(List<string> errors) => new()
        {
            IsSuccess = false,
            Errors = errors
        };
    }
}
