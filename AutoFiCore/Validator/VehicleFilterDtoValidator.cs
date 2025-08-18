using AutoFiCore.Dto;
using FluentValidation;

public class VehicleFilterDtoValidator : AbstractValidator<VehicleFilterDto>
{
    public VehicleFilterDtoValidator()
    {
        RuleFor(x => x.Make)
               .NotEmpty().WithMessage("'Make' is required.")
               .MaximumLength(50).WithMessage("'Make' cannot exceed 50 characters.")
               .Matches(@"^[A-Za-z\s]+$").WithMessage("'Make' must only contain letters and spaces.");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("'Model' is required.")
            .MaximumLength(50).WithMessage("'Model' cannot exceed 50 characters.")
            .Matches(@"^[A-Za-z0-9\s]+$").WithMessage("'Model' must only contain letters, numbers, and spaces.");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).When(x => x.Mileage.HasValue)
            .WithMessage("'Mileage' must be greater than 0.");

        RuleFor(x => x.EndPrice)
            .GreaterThanOrEqualTo(x => x.StartPrice)
            .When(x => x.StartPrice.HasValue && x.EndPrice.HasValue)
            .WithMessage("'EndPrice' must be >= 'StartPrice'.");

        RuleFor(x => x.EndYear)
            .GreaterThanOrEqualTo(x => x.StartYear)
            .When(x => x.StartYear.HasValue && x.EndYear.HasValue)
            .WithMessage("'EndYear' must be >= 'StartYear'.");
    }
}
