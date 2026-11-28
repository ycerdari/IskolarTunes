using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace IskolarTunes.Services;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = "IskolarTunes";
}

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly IHostEnvironment _env;

    public SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger, IHostEnvironment env)
    {
        _settings = settings.Value;
        _logger = logger;
        _env = env;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host)
            || string.IsNullOrWhiteSpace(_settings.User)
            || string.IsNullOrWhiteSpace(_settings.Password))
        {
            // Not configured: never throw. In Development, show the email (and its link) in the log.
            if (_env.IsDevelopment())
                _logger.LogWarning("SMTP is not configured. Email to {Email} not sent. Subject: {Subject}. Body: {Body}", email, subject, htmlMessage);
            else
                _logger.LogWarning("SMTP is not configured. Email to {Email} not sent. Subject: {Subject}", email, subject);
            return;
        }

        var from = string.IsNullOrWhiteSpace(_settings.From) ? _settings.User : _settings.From;

        using var message = new MailMessage
        {
            From = new MailAddress(from, _settings.FromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        message.To.Add(email);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_settings.User, _settings.Password)
        };

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Don't reveal to the visitor whether sending worked; log it instead.
            _logger.LogError(ex, "Failed to send email to {Email}", email);
        }
    }
}