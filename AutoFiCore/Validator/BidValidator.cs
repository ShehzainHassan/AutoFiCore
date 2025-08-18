using FluentValidation;

public class BidValidationDTO
{
    public decimal Amount { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public int BidCount { get; set; }
}

public class BidValidator : AbstractValidator<BidValidationDTO>
{
    public BidValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Bid must be greater than 0.")
            .Custom((amount, context) =>
            {
                var minIncrement = BidIncrementCalculator.GetMinimumIncrement(context.InstanceToValidate.CurrentPrice, context.InstanceToValidate.BidCount);
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
