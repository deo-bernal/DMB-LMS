using System.Net;
using System.Net.Mail;
using Dmb.Lms.Model.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dmb.Lms.Service.Implementation.Email;

public class EmailService : IActivationEmailSender, IPasswordResetEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task SendAccountActivationEmailAsync(string toEmail, string activationLink, CancellationToken cancellationToken = default)
    {
        var html = $@"<h2>Activate your DMB LMS account</h2>
<p>Click the link below to activate your account.</p>
<p><a href=""{WebUtility.HtmlEncode(activationLink)}"">Activate account</a></p>";
        return SendAsync(toEmail, "Activate your account", html, cancellationToken);
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        var html = $@"<h2>Reset your DMB LMS password</h2>
<p>Click the link below to choose a new password. It expires in one hour.</p>
<p><a href=""{WebUtility.HtmlEncode(resetLink)}"">Reset password</a></p>";
        return SendAsync(toEmail, "Password reset request", html, cancellationToken);
    }

    private async Task SendAsync(string toEmail, string subject, string html, CancellationToken cancellationToken)
    {
        var host = _configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("SMTP is not configured. Email to {Email} subject {Subject} was not sent.", toEmail, subject);
            throw new InvalidOperationException("Email service is not configured.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_configuration["Smtp:From"] ?? "noreply@dmbwebsolutions.com", "DMB LMS"),
            Subject = subject,
            Body = html,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);
        using var client = new SmtpClient(host, int.Parse(_configuration["Smtp:Port"] ?? "587")) { EnableSsl = true };
        var user = _configuration["Smtp:Username"];
        if (!string.IsNullOrWhiteSpace(user))
        {
            client.Credentials = new NetworkCredential(user, _configuration["Smtp:Password"]);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }
}
