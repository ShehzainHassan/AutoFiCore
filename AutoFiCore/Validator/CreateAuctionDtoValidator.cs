using AutoFiCore.Dto;
using FluentValidation;

/// <summary>
/// Provides validation rules for the <see cref="CreateAuctionDTO"/> used when creating a new auction.
/// </summary>
public class CreateAuctionDtoValidator : AbstractValidator<CreateAuctionDTO>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateAuctionDtoValidator"/> class
    /// and defines validation rules for auction creation parameters.
    /// </summary>
    public CreateAuctionDtoValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0).WithMessage("VehicleId must be greater than 0.");

        RuleFor(x => x.ScheduledStartTime)
            .NotEmpty().WithMessage("ScheduledStartTime cannot be empty.");

        RuleFor(x => x.EndUtc)
            .NotEmpty().WithMessage("EndUtc cannot be empty.")
            .GreaterThan(x => x.ScheduledStartTime).WithMessage("EndUtc must be > ScheduledStartTime.");

        RuleFor(x => x.StartingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Starting price must be non-negative.");

        RuleFor(x => x.PreviewStartTime)
            .LessThan(x => x.ScheduledStartTime).WithMessage("PreviewTime must be earlier than ScheduledStartTime");

        RuleFor(x => x.ReservePrice)
            .GreaterThanOrEqualTo(x => x.StartingPrice)
            .When(x => x.ReservePrice.HasValue)
            .WithMessage("ReservePrice must be greater than or equal to StartingPrice");
    }
}