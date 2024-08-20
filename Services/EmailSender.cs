using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (_configuration == null)
            return;

        var smtpClient = new SmtpClient(_configuration["SmtpHost"])
        {
            Port = int.Parse(_configuration["SmtpPort"] ?? throw new InvalidOperationException("SMTP Port is not configured.")),
            Credentials = new NetworkCredential(_configuration["SmtpUsername"], _configuration["SmtpPassword"]),
            EnableSsl = true, // Ensure SSL is enabled
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["SmtpFrom"] ?? throw new InvalidOperationException("SMTP From is not configured.")),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true,
        };

        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
