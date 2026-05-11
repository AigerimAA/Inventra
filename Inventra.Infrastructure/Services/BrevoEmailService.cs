using Inventra.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Inventra.Infrastructure.Services
{
    public class BrevoEmailService : IEmailService
    {
        private readonly string _login;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly ILogger<BrevoEmailService> _logger;

        public BrevoEmailService(IConfiguration configuration, ILogger<BrevoEmailService> logger)
        {
            _login = configuration["Brevo:Login"]!;
            _smtpPassword = configuration["Brevo:SmtpPassword"]!;
            _fromEmail = configuration["Brevo:FromEmail"]!;
            _logger = logger;
        }

        public async Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink)
        {
            try
            {
                _logger.LogInformation("Sending confirmation email to {Email}", toEmail);
                using var client = new SmtpClient("smtp-relay.brevo.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_login, _smtpPassword)
                };
                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, "Inventra"),
                    Subject = "Confirm your Inventra account",
                    IsBodyHtml = true,
                    Body = $"""
                        <h2>Welcome to Inventra, {userName}!</h2>
                        <p>Please confirm your email address by clicking the link below:</p>
                        <p><a href="{confirmationLink}" style="background:#0097a7;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">Confirm Email</a></p>
                        <p>If you didn't register, ignore this email.</p>
                        """
                };
                message.To.Add(new MailAddress(toEmail, userName));
                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
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
                using var client = new SmtpClient("smtp-relay.brevo.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_login, _smtpPassword)
                };
                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, "Inventra"),
                    Subject = "Reset your Inventra password",
                    IsBodyHtml = true,
                    Body = $"""
                        <h2>Password Reset</h2>
                        <p>Hi {userName}, click below to reset your password:</p>
                        <p><a href="{resetLink}" style="background:#0097a7;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">Reset Password</a></p>
                        <p>If you didn't request this, ignore this email.</p>
                        """
                };
                message.To.Add(new MailAddress(toEmail, userName));
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            }
        }
    }
}
