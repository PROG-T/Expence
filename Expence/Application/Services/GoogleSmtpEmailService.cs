using Expence.Application.Interface;
using Expence.Domain.OptionsConfiguration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Expence.Application.Services
{
    public class GoogleSmtpEmailService : ISmtpEmailService
    {
        private readonly EmailSettingsOptions _emailSettings;
        private readonly ILogger<GoogleSmtpEmailService> _logger;

        public GoogleSmtpEmailService(
            IOptions<EmailSettingsOptions> emailSettings,
            ILogger<GoogleSmtpEmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent)
        {
            try
            {
                _logger.LogInformation(
                    "Sending email via Google SMTP. Recipient: {Recipient}, Subject: {Subject}",
                    to, subject);

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword)
                };

                using var mailMessage = new MailMessage(
                    new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    new MailAddress(to))
                {
                    Subject = subject,
                    Body = htmlContent,
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation(
                    "Email sent successfully. Recipient: {Recipient}, Subject: {Subject}",
                    to, subject);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send email via Google SMTP. Recipient: {Recipient}, Subject: {Subject}",
                    to, subject);

                return false;
            }
        }

        public async Task<bool> SendEmailVerificationEmailAsync(string email, string verificationToken)
        {
            try
            {
                _logger.LogInformation("Sending email verification to: {Email}", email);

                var htmlContent = EmailTemplateService.GetEmailVerificationTemplate(
                    email, verificationToken, _emailSettings.FrontendUrl);

                return await SendEmailAsync(email, "Verify Your Expence Email", htmlContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email verification to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken)
        {
            try
            {
                _logger.LogInformation("Sending password reset email to: {Email}", email);

                var htmlContent = EmailTemplateService.GetPasswordResetEmailTemplate(
                    email, resetToken, _emailSettings.FrontendUrl);

                return await SendEmailAsync(email, "Reset Your Expence Password", htmlContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("Sending welcome email to: {Email}", email);

                var htmlContent = EmailTemplateService.GetWelcomeEmailTemplate(email);

                return await SendEmailAsync(email, "Welcome to Expence!", htmlContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
                return false;
            }
        }
    }
}
