namespace Expence.Application.Interface
{
    public interface ISmtpEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken);
        Task<bool> SendWelcomeEmailAsync(string email);
        Task<bool> SendEmailVerificationEmailAsync(string email, string verificationToken);
        Task<bool> SendEmailAsync(string to, string subject, string htmlContent);

    }
}
