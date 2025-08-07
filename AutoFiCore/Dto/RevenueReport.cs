namespace AutoFiCore.Dto
{
    public class RevenueReport
    {
        public decimal TotalRevenue { get; set; }
        public decimal CommissionEarned { get; set; }
        public decimal AverageSalePrice { get; set; }
        public decimal SuccessfulPaymentsPercentage { get; set; }
    }
}
