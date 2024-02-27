using MimeKit;
using WebApi.Services.Interfaces;
using MailKit.Security;

namespace CandidateDashboardApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
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

            using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await smtpClient.ConnectAsync(_configuration["Email:SmtpHost"], int.Parse(_configuration["Email:SmtpPort"]!), SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_configuration["Email:From"], _configuration["Email:Password"]);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true, default);

            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }
    }
}
