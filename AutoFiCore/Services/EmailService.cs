using System.Net.Mail;
using System.Net;

public interface IEmailService
{
    Task SendLoanEmailAsync(string toEmail, byte[] pdfAttachment);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendLoanEmailAsync(string toEmail, byte[] pdfAttachment)
    {
        var message = new MailMessage();
        message.From = new MailAddress("boxcars.autofi@gmail.com", "BoxCars");
        message.To.Add(toEmail);
        message.Subject = "Your Loan Offer Summary";
        message.Body = "Please find attached your loan offer details in PDF format.";
        message.Attachments.Add(new Attachment(new MemoryStream(pdfAttachment), "LoanSummary.pdf", "application/pdf"));

        using var smtp = new SmtpClient("smtp.gmail.com")
        {
            Credentials = new NetworkCredential("boxcars.autofi@gmail.com", "bbzu ivkc ksua kqvh"),
            EnableSsl = true,
            Port = 587
        };
        await smtp.SendMailAsync(message);
    }
}
