namespace AutoFiCore.Data.Interfaces
{
    public interface IEmailService
    {
        Task SendLoanEmailAsync(string toEmail, byte[] pdfAttachment);
    }
}
