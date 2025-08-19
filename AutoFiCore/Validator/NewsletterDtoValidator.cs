using AutoFiCore.Dto;
using AutoFiCore.Models;
using FluentValidation;

namespace AutoFiCore.Validator
{
    /// <summary>
    /// Provides validation rules for the <see cref="Newsletter"/> model.
    /// </summary>
    public class NewsletterDtoValidator : AbstractValidator<Newsletter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewsletterDtoValidator"/> class
        /// and defines validation rules for newsletter subscription properties.
        /// </summary>
        public NewsletterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("'Email' is required.")
                .EmailAddress().WithMessage("'Email' is not in a valid format.");
        }
    }
}