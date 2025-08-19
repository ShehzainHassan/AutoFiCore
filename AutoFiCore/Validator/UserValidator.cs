using AutoFiCore.Models;
using FluentValidation;

/// <summary>
/// Provides validation rules for the <see cref="User"/> model.
/// </summary>
public class UserValidator : AbstractValidator<User>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserValidator"/> class
    /// and defines validation rules for user properties.
    /// </summary>
    public UserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not in a valid format.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Matches(@"^[A-Za-z\s]+$").WithMessage("Name must only contain letters and spaces.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
            .WithMessage("Password must be at least 8 characters long and include upper, lower, number, and special char.");
    }
}