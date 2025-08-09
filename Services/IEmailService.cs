namespace BudgetSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlContent);
        Task SendVerificationEmailAsync(string toEmail, string userName, string verificationLink);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
    }
} 