using AutoFiCore.Enums;
using AutoFiCore.Models;

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

    public static decimal GetMinimumIncrement(decimal currentPrice, int bidCount)
    {
        decimal baseIncrement = GetMinimumIncrement(currentPrice);

        if (bidCount >= 20)
            return baseIncrement * 2m;

        if (bidCount >= 10)
            return baseIncrement * 1.5m;

        return baseIncrement;
    }

    public static decimal GetIncrementByStrategy(decimal currentPrice, int bidCount, BidStrategyType strategy)
    {
        decimal minInc = GetMinimumIncrement(currentPrice, bidCount);
        switch (strategy)
        {
            case BidStrategyType.Aggressive:
                return minInc * 2m;
            case BidStrategyType.Incremental:
                return minInc * 1.25m;
            case BidStrategyType.Conservative:
            default:
                return minInc;
        }
    }
}
