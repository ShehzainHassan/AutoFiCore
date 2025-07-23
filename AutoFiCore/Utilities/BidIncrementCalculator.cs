using AutoFiCore.Enums;

public static class BidIncrementCalculator
{
    public static decimal GetMinimumIncrement(decimal currentPrice)
    {
        if (currentPrice < 1000) return 50m;
        if (currentPrice < 5000) return 100m;
        if (currentPrice < 10000) return 250m;
        if (currentPrice < 25000) return 500m;
        return 1000m;
    }
    public static decimal GetMinimumIncrement(decimal currentPrice, int bidCount, BidStrategyType strategy)
    {
        decimal baseIncrement = GetMinimumIncrement(currentPrice);

        switch (strategy)
        {
            case BidStrategyType.Conservative:
                return baseIncrement;

            case BidStrategyType.Aggressive:
                if (bidCount >= 20) return baseIncrement * 3m;
                if (bidCount >= 10) return baseIncrement * 2.5m;
                return baseIncrement * 2m;

            case BidStrategyType.Incremental:
                if (bidCount >= 20) return baseIncrement * 2m;
                if (bidCount >= 10) return baseIncrement * 1.5m;
                return baseIncrement;

            default:
                return baseIncrement;
        }
    }

    public static decimal GetMinimumIncrement(decimal currentPrice, int bidCount)
    {
        return GetMinimumIncrement(currentPrice, bidCount, BidStrategyType.Conservative);
    }
    public static decimal GetIncrementByStrategy(decimal currentPrice, int bidCount, BidStrategyType strategy)
    {
        return GetMinimumIncrement(currentPrice, bidCount, strategy);
    }
}
