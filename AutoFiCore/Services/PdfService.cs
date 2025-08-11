using AutoFiCore.Dto;
using AutoFiCore.Models;
using QuestPDF.Fluent;

public interface IPdfService
{
    byte[] GenerateLoanPdf(Questionnaire questionnaire, LoanCalculation loan);
    byte[] GenerateAuctionPerformancePdf(DateTime startDate, DateTime endDate, AuctionPerformanceReport report);
    byte[] GenerateUserActivityPdf(DateTime startDate, DateTime endDate, UserActivityReport report);
    byte[] GenerateRevenueReportPdf(DateTime startDate, DateTime endDate, decimal revenue);
    byte[] GenerateDashboardSummaryPdf(DateTime startDate, DateTime endDate, ExecutiveDashboard report);
}



public class PdfService : IPdfService
{
    public byte[] GenerateLoanPdf(Questionnaire questionnaire, LoanCalculation loan)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Content()
                    .Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text("Loan Offer Summary").FontSize(20).Bold().Underline();
                        col.Item().Text($"Borrow Amount: {loan.LoanAmount}");
                        col.Item().Text($"Monthly Payment: {loan.MonthlyPayment}");
                        col.Item().Text($"Total Cost: {loan.TotalCost}");
                        col.Item().Text($"Interest Rate: {loan.InterestRate}%");
                        col.Item().Text($"Term: {loan.LoanTermMonths} months");
                    });
            });
        }).GeneratePdf();
    }
    public byte[] GenerateDashboardSummaryPdf(DateTime startDate, DateTime endDate, ExecutiveDashboard report)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Content()
                    .Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text("Dashboard Summary Report").FontSize(20).Bold().Underline();
                        col.Item().Text($"Start Date: {startDate:yyyy-MM-dd}");
                        col.Item().Text($"End Date: {endDate:yyyy-MM-dd}");
                        col.Item().Text($"Total Revenue: {report.TotalRevenue:C}");
                        col.Item().Text($"New Users: {report.NewUsers}");
                        col.Item().Text($"Active Auctions: {report.ActiveAuctions}");
                    });
            });
        }).GeneratePdf();
    }
    public byte[] GenerateAuctionPerformancePdf(DateTime startDate, DateTime endDate, AuctionPerformanceReport report)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Content()
                    .Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text("Auction Performance Report").FontSize(20).Bold().Underline();
                        col.Item().Text($"Start Date: {startDate:yyyy-MM-dd}");
                        col.Item().Text($"End Date: {endDate:yyyy-MM-dd}");
                        col.Item().Text($"Total Auctions: {report.TotalAuctions}");
                        col.Item().Text($"Success Rate: {report.SuccessRate:F2}%");
                        col.Item().Text($"Average Final Price: {report.AverageFinalPrice:C}");
                    });
            });
        }).GeneratePdf();
    }
    public byte[] GenerateUserActivityPdf(DateTime startDate, DateTime endDate, UserActivityReport report)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Content()
                    .Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text("User Activity Report").FontSize(20).Bold().Underline();
                        col.Item().Text($"Start Date: {startDate:yyyy-MM-dd}");
                        col.Item().Text($"End Date: {endDate:yyyy-MM-dd}");
                        col.Item().Text($"New Registrations: {report.NewRegistrations}");
                        col.Item().Text($"Engagement Score: {report.EngagementScore}");
                    });
            });
        }).GeneratePdf();
    }
    public byte[] GenerateRevenueReportPdf(DateTime startDate, DateTime endDate, decimal revenue)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Content()
                    .Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text("Revenue Report").FontSize(20).Bold().Underline();
                        col.Item().Text($"Start Date: {startDate:yyyy-MM-dd}");
                        col.Item().Text($"End Date: {endDate:yyyy-MM-dd}");
                        col.Item().Text($"Commission Earned: {revenue:C}");
                    });
            });
        }).GeneratePdf();
    }
}
