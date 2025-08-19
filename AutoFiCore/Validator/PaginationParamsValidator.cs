using AutoFiCore.Models;
using FluentValidation;

namespace AutoFiCore.Validator
{
    /// <summary>
    /// Validates pagination parameters to ensure they meet expected constraints.
    /// </summary>
    public class PaginationParamsValidator : AbstractValidator<PaginationParams>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationParamsValidator"/> class
        /// and defines validation rules for pagination properties.
        /// </summary>
        public PaginationParamsValidator()
        {
            RuleFor(x => x.PageView)
                .GreaterThan(0)
                .WithMessage("'PageView' must be greater than 0.");

            RuleFor(x => x.Offset)
                .GreaterThanOrEqualTo(0)
                .WithMessage("'Offset' cannot be negative.");
        }
    }
}