using FluentValidation;

/// <summary>
/// Represents the data required to validate a bid in an auction.
/// </summary>
public class BidValidationDTO
{
    /// <summary>
    /// The amount of the bid placed by the user.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The initial starting price of the auction.
    /// </summary>
    public decimal StartingPrice { get; set; }

    /// <summary>
    /// The current highest bid in the auction.
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// The total number of bids already placed.
    /// </summary>
    public int BidCount { get; set; }
}

/// <summary>
/// Provides validation rules for the <see cref="BidValidationDTO"/> used to ensure bid integrity.
/// </summary>
public class BidValidator : AbstractValidator<BidValidationDTO>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BidValidator"/> class
    /// and defines custom validation logic for bid amount.
    /// </summary>
    public BidValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Bid must be greater than 0.")
            .Custom((amount, context) =>
            {
                var minIncrement = BidIncrementCalculator.GetMinimumIncrement(
                    context.InstanceToValidate.CurrentPrice,
                    context.InstanceToValidate.BidCount);

                var minBid = context.InstanceToValidate.CurrentPrice > 0
                    ? context.InstanceToValidate.CurrentPrice + minIncrement
                    : context.InstanceToValidate.StartingPrice;

                if (amount < minBid)
                {
                    context.AddFailure($"Bid must be at least {minBid:C}.");
                }
            });
    }
}