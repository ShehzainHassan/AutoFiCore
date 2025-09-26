using AutoFiCore.Data.Interfaces;
using AutoFiCore.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _emailSettings = options.Value;
    }

    public async Task SendLoanEmailAsync(string toEmail, byte[] pdfAttachment)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_emailSettings.Address, "BoxCars"),
            Subject = "Your Loan Offer Summary",
            Body = "Please find attached your loan offer details in PDF format."
        };
        message.To.Add(toEmail);

        message.Attachments.Add(new Attachment(new MemoryStream(pdfAttachment),
                                               "LoanSummary.pdf",
                                               "application/pdf"));

        using var smtp = new SmtpClient("smtp.gmail.com")
        {
            Credentials = new NetworkCredential(_emailSettings.Address, _emailSettings.Password),
            EnableSsl = true,
            Port = 587
        };

        await smtp.SendMailAsync(message);
    }
}
