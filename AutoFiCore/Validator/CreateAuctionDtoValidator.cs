using AutoFiCore.Dto;
using FluentValidation;

public class CreateAuctionDtoValidator : AbstractValidator<CreateAuctionDTO>
{
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
