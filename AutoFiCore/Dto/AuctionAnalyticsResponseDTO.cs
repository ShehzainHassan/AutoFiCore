namespace AutoFiCore.Dto
{
    public class AuctionAnalyticsResponseDTO
    {
        public List<AuctionAnalyticsTableDTO> CurrentPeriodData { get; set; } = new();
        public double PercentageChange { get; set; }
    }
}
