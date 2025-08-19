using AutoFiCore.Enums;

/// <summary>
/// Provides logic to calculate minimum bid increments based on current price, bid count, and bidding strategy.
/// </summary>
public static class BidIncrementCalculator
{
    /// <summary>
    /// Gets the base minimum increment based solely on the current price.
    /// </summary>
    /// <param name="currentPrice">The current bid price.</param>
    /// <returns>The minimum increment value.</returns>
    public static decimal GetMinimumIncrement(decimal currentPrice)
    {
        if (currentPrice < 1000) return 50m;
        if (currentPrice < 5000) return 100m;
        if (currentPrice < 10000) return 250m;
        if (currentPrice < 25000) return 500m;
        return 1000m;
    }

    /// <summary>
    /// Gets the minimum increment based on price, bid count, and strategy type.
    /// </summary>
    /// <param name="currentPrice">The current bid price.</param>
    /// <param name="bidCount">The number of bids placed so far.</param>
    /// <param name="strategy">The bidding strategy to apply.</param>
    /// <returns>The adjusted minimum increment value.</returns>
    public static decimal GetMinimumIncrement(decimal currentPrice, int bidCount, BidStrategyType strategy)
    {
        decimal baseIncrement = GetMinimumIncrement(currentPrice);

        return strategy switch
        {
            BidStrategyType.Conservative => baseIncrement,
            BidStrategyType.Aggressive => bidCount switch
            {
                >= 20 => baseIncrement * 3m,
                >= 10 => baseIncrement * 2.5m,
                _ => baseIncrement * 2m
            },
            BidStrategyType.Incremental => bidCount switch
            {
                >= 20 => baseIncrement * 2m,
                >= 10 => baseIncrement * 1.5m,
                _ => baseIncrement
            },
            _ => baseIncrement
        };
    }

    /// <summary>
    /// Gets the minimum increment using the default Conservative strategy.
    /// </summary>
    /// <param name="currentPrice">The current bid price.</param>
    /// <param name="bidCount">The number of bids placed so far.</param>
    /// <returns>The minimum increment value.</returns>
    public static decimal GetMinimumIncrement(decimal currentPrice, int bidCount)
    {
        return GetMinimumIncrement(currentPrice, bidCount, BidStrategyType.Conservative);
    }

    /// <summary>
    /// Alias for <see cref="GetMinimumIncrement(decimal, int, BidStrategyType)"/> for semantic clarity.
    /// </summary>
    /// <param name="currentPrice">The current bid price.</param>
    /// <param name="bidCount">The number of bids placed so far.</param>
    /// <param name="strategy">The bidding strategy to apply.</param>
    /// <returns>The adjusted minimum increment value.</returns>
    public static decimal GetIncrementByStrategy(decimal currentPrice, int bidCount, BidStrategyType strategy)
    {
        return GetMinimumIncrement(currentPrice, bidCount, strategy);
    }
}