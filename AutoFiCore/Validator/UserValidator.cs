using AutoFiCore.Models;
using FluentValidation;

public class UserValidator : AbstractValidator<User>
{
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
