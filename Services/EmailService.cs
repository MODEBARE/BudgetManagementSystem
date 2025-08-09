using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;

namespace BudgetSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {
                _logger.LogInformation($"Attempting to send email to {toEmail} with subject: {subject}");
                _logger.LogInformation($"SMTP Settings - Server: {_emailSettings.SmtpServer}, Port: {_emailSettings.SmtpPort}, Username: {_emailSettings.Username}");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlContent
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // Enable logging for SMTP client
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                
                _logger.LogInformation("Connecting to SMTP server...");
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.SslOnConnect);
                
                _logger.LogInformation("Authenticating...");
                if (!string.IsNullOrEmpty(_emailSettings.Username))
                {
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                }

                _logger.LogInformation("Sending email...");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}. Error: {ex.Message}");
                throw;
            }
        }

        public async Task SendVerificationEmailAsync(string toEmail, string userName, string verificationLink)
        {
            var subject = "Verify Your Budget System Account";
            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center;'>
                        <h1 style='color: #007bff; margin: 0;'>Welcome to Budget System</h1>
                    </div>
                    <div style='padding: 30px 20px;'>
                        <h2 style='color: #333;'>Hello {userName},</h2>
                        <p style='color: #666; font-size: 16px; line-height: 1.6;'>
                            Thank you for registering with Budget System! To complete your registration, 
                            please verify your email address by clicking the button below:
                        </p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{verificationLink}' 
                               style='background-color: #007bff; color: white; padding: 12px 30px; 
                                      text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Verify Email Address
                            </a>
                        </div>
                        <p style='color: #666; font-size: 14px;'>
                            If the button doesn't work, you can copy and paste the following link into your browser:
                        </p>
                        <p style='color: #007bff; word-break: break-all; font-size: 14px;'>
                            {verificationLink}
                        </p>
                        <p style='color: #666; font-size: 14px; margin-top: 30px;'>
                            If you didn't create an account with Budget System, please ignore this email.
                        </p>
                    </div>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
                        <p>© {DateTime.Now.Year} Budget System. All rights reserved.</p>
                    </div>
                </div>";

            await SendEmailAsync(toEmail, subject, htmlContent);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to Budget System - Your Account is Ready!";
            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #28a745; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>Welcome to Budget System!</h1>
                    </div>
                    <div style='padding: 30px 20px;'>
                        <h2 style='color: #333;'>Hello {userName},</h2>
                        <p style='color: #666; font-size: 16px; line-height: 1.6;'>
                            Congratulations! Your email has been verified and your Budget System account is now active.
                        </p>
                        <p style='color: #666; font-size: 16px; line-height: 1.6;'>
                            You can now start managing your finances with our comprehensive budgeting tools:
                        </p>
                        <ul style='color: #666; font-size: 16px; line-height: 1.8;'>
                            <li>Track income and expenses</li>
                            <li>Create and monitor budgets</li>
                            <li>Set financial goals</li>
                            <li>Generate detailed reports</li>
                            <li>Receive bill reminders</li>
                        </ul>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='#' 
                               style='background-color: #28a745; color: white; padding: 12px 30px; 
                                      text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Get Started Now
                            </a>
                        </div>
                        <p style='color: #666; font-size: 14px; margin-top: 30px;'>
                            If you have any questions, feel free to contact our support team.
                        </p>
                    </div>
                    <div style='background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666;'>
                        <p>© {DateTime.Now.Year} Budget System. All rights reserved.</p>
                    </div>
                </div>";

            await SendEmailAsync(toEmail, subject, htmlContent);
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public bool UseSsl { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }
} 