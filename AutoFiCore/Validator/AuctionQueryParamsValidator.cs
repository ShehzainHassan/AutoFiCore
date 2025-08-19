using AutoFiCore.Dto;
using FluentValidation;

/// <summary>
/// Provides validation rules for the <see cref="AuctionQueryParams"/> DTO used in auction filtering.
/// </summary>
public class AuctionQueryParamsValidator : AbstractValidator<AuctionQueryParams>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuctionQueryParamsValidator"/> class
    /// and defines validation rules for minimum and maximum price constraints.
    /// </summary>
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