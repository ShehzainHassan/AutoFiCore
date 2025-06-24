using AutoFiCore.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public interface IPdfService
{
    byte[] GenerateLoanPdf(Questionnaire questionnaire, LoanCalculation loan);
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
}
