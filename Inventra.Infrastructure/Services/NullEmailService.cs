using Inventra.Application.Interfaces;

namespace Inventra.Infrastructure.Services
{
    public class NullEmailService : IEmailService
    {
        public Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink)
            => Task.CompletedTask;

        public Task SendPasswordResetAsync(string toEmail, string userName, string resetLink)
            => Task.CompletedTask;
    }
}
