using AutoFiCore.Models;
using FluentValidation;

namespace AutoFiCore.Validator
{
    public class PaginationParamsValidator : AbstractValidator<PaginationParams>
    {
        public PaginationParamsValidator()
        {
            RuleFor(x => x.PageView).GreaterThan(0).WithMessage("'PageView' must be greater than 0.");
            RuleFor(x => x.Offset).GreaterThanOrEqualTo(0).WithMessage("'Offset' cannot be negative.");
        }
    }
}
