using Microsoft.Extensions.Logging;

namespace BudgetSystem.Services
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            _logger.LogInformation("=== MOCK EMAIL SENT ===");
            _logger.LogInformation("To: {ToEmail}", toEmail);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Content: {Content}", htmlContent);
            _logger.LogInformation("=== END MOCK EMAIL ===");
            
            // Simulate email sending delay
            return Task.Delay(100);
        }

        public async Task SendVerificationEmailAsync(string toEmail, string userName, string verificationLink)
        {
            var subject = "Verify Your Budget System Account";
            var htmlContent = $@"
                <h2>Hello {userName},</h2>
                <p>Thank you for registering with Budget System!</p>
                <p>To complete your registration, please click the link below:</p>
                <p><a href='{verificationLink}'>Verify Email Address</a></p>
                <p>Or copy and paste this link: {verificationLink}</p>
                <p>If you didn't create an account, please ignore this email.</p>";

            await SendEmailAsync(toEmail, subject, htmlContent);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to Budget System - Your Account is Ready!";
            var htmlContent = $@"
                <h2>Welcome {userName}!</h2>
                <p>Your email has been verified and your Budget System account is now active.</p>
                <p>You can now start managing your finances with our comprehensive budgeting tools.</p>
                <p>Get started now!</p>";

            await SendEmailAsync(toEmail, subject, htmlContent);
        }
    }
} 