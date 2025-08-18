using AutoFiCore.Dto;
using AutoFiCore.Models;
using FluentValidation;

namespace AutoFiCore.Validator
{

    public class NewsletterDtoValidator : AbstractValidator<Newsletter>
    {
        public NewsletterDtoValidator()
        {
             RuleFor(x => x.Email)
                .NotEmpty().WithMessage("'Email' is required.")
                .EmailAddress().WithMessage("'Email' is not in a valid format.");
        }
    }
}
