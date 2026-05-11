using Inventra.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Inventra.Infrastructure.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
        {
            _apiKey = configuration["SendGrid:ApiKey"]!;
            _fromEmail = configuration["SendGrid:FromEmail"]!;
            _fromName = configuration["SendGrid:FromName"] ?? "Inventra";
            _logger = logger;
        }

        public async Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink)
        {
            try
            {
                _logger.LogInformation("Sending confirmation email to {Email}", toEmail);
                var client = new SendGridClient(_apiKey);
                var msg = new SendGridMessage
                {
                    From = new EmailAddress(_fromEmail, _fromName),
                    Subject = "Confirm your Inventra account"
                };
                msg.AddTo(new EmailAddress(toEmail, userName));
                msg.HtmlContent = $"""
                    <h2>Welcome to Inventra, {userName}!</h2>
                    <p>Please confirm your email address by clicking the link below:</p>
                    <p><a href="{confirmationLink}" style="background:#0097a7;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">Confirm Email</a></p>
                    <p>If you didn't register, ignore this email.</p>
                    """;
                msg.PlainTextContent = $"Confirm your email: {confirmationLink}";

                var response = await client.SendEmailAsync(msg);
                _logger.LogInformation("SendGrid response: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Body.ReadAsStringAsync();
                    _logger.LogError("SendGrid error: {StatusCode} - {Body}", response.StatusCode, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", toEmail);
            }
        }

        public async Task SendPasswordResetAsync(string toEmail, string userName, string resetLink)
        {
            try
            {
                var client = new SendGridClient(_apiKey);
                var msg = new SendGridMessage
                {
                    From = new EmailAddress(_fromEmail, _fromName),
                    Subject = "Reset your Inventra password"
                };
                msg.AddTo(new EmailAddress(toEmail, userName));
                msg.HtmlContent = $"""
                    <h2>Password Reset</h2>
                    <p>Hi {userName}, click below to reset your password:</p>
                    <p><a href="{resetLink}" style="background:#0097a7;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">Reset Password</a></p>
                    <p>If you didn't request this, ignore this email.</p>
                    """;
                msg.PlainTextContent = $"Reset your password: {resetLink}";
                await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            }
        }
    }
}
