using IMS.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace IMS.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MailMessage();
            message.To.Add(new MailAddress(toEmail));
            message.From = new MailAddress(_settings.SenderEmail, _settings.SenderName);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using (var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort))
            {
                client.Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword);
                client.EnableSsl = true;
                await client.SendMailAsync(message);
            }
        }
    }

}
