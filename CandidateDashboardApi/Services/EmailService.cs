using MimeKit;
using WebApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Security;
using System;
using System.Threading.Tasks;

namespace CandidateDashboardApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_configuration["Email:AppName"], _configuration["Email:From"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            message.Body = builder.ToMessageBody();

            using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    await smtpClient.ConnectAsync(_configuration["Email:SmtpHost"], int.Parse(_configuration["Email:SmtpPort"]!), SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(_configuration["Email:From"], _configuration["Email:Password"]);
                    await smtpClient.SendAsync(message);
                    await smtpClient.DisconnectAsync(true, default);

                    _logger.LogInformation("Email sent successfully to: {EmailAddress}", email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending email to: {EmailAddress}", email);
                }
            }
        }
    }
}