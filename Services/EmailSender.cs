using System.Net;
using System.Net.Mail;

namespace EagleFlow.Services;

public class EmailSender(IConfiguration configuration, ILogger<EmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string toEmail, string subject, string body)
    {
        var smtpHost = configuration["Smtp:Host"];
        var smtpPort = int.TryParse(configuration["Smtp:Port"], out var port) ? port : 25;
        var fromEmail = configuration["Smtp:FromEmail"];
        var fromName = configuration["Smtp:FromName"] ?? "EagleFlow";
        var smtpUser = configuration["Smtp:Username"];
        var smtpPass = configuration["Smtp:Password"];

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(fromEmail))
        {
            logger.LogInformation("SMTP not configured. Simulated email to {Email}. Subject: {Subject}. Body: {Body}", toEmail, subject, body);
            return;
        }

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = string.IsNullOrWhiteSpace(smtpUser)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(smtpUser, smtpPass)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(toEmail);
        await client.SendMailAsync(message);
    }
}
