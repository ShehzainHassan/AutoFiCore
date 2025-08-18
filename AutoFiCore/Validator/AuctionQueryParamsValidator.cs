using AutoFiCore.Dto;
using FluentValidation;

public class AuctionQueryParamsValidator : AbstractValidator<AuctionQueryParams>
{
    public AuctionQueryParamsValidator()
    {
        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue)
            .WithMessage("'MinPrice' must be >= 0");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithMessage("'MaxPrice' must be >= MinPrice");
    }
}
