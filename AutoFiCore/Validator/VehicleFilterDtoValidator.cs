using AutoFiCore.Dto;
using FluentValidation;


/// <summary>
/// Validator for <see cref="VehicleFilterDto"/> that enforces basic constraints 
/// such as maximum length, character rules, and valid ranges. 
/// All fields are optional, but when provided they must match the rules.
/// </summary>
public class VehicleFilterDtoValidator : AbstractValidator<VehicleFilterDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleFilterDtoValidator"/> class 
    /// and defines validation rules for <see cref="VehicleFilterDto"/>.
    /// </summary>
    public VehicleFilterDtoValidator()
    {
        RuleFor(x => x.Make)
            .MaximumLength(50).WithMessage("'Make' cannot exceed 50 characters.")
            .Matches(@"^[A-Za-z\s]+$").When(x => !string.IsNullOrWhiteSpace(x.Make))
            .WithMessage("'Make' must only contain letters and spaces.");

        RuleFor(x => x.Model)
            .MaximumLength(50).WithMessage("'Model' cannot exceed 50 characters.")
            .Matches(@"^[A-Za-z0-9\s]+$").When(x => !string.IsNullOrWhiteSpace(x.Model))
            .WithMessage("'Model' must only contain letters, numbers, and spaces.");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).When(x => x.Mileage.HasValue)
            .WithMessage("'Mileage' must be greater than or equal to 0.");

        RuleFor(x => x.EndPrice)
            .GreaterThanOrEqualTo(x => x.StartPrice)
            .When(x => x.StartPrice.HasValue && x.EndPrice.HasValue)
            .WithMessage("'EndPrice' must be greater than or equal to 'StartPrice'.");

        RuleFor(x => x.EndYear)
            .GreaterThanOrEqualTo(x => x.StartYear)
            .When(x => x.StartYear.HasValue && x.EndYear.HasValue)
            .WithMessage("'EndYear' must be greater than or equal to 'StartYear'.");
    }
}
