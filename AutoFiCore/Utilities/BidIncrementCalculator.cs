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
        if (bidCount == 0)
            return currentPrice;
        decimal baseIncrement = GetMinimumIncrement(currentPrice);

        if (bidCount >= 20)
            return baseIncrement * 2m;

        if (bidCount >= 10)
            return baseIncrement * 1.5m;

        return baseIncrement;
    }
}
