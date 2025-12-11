// Dede.Service/Services/EmailSender.cs
using System.Net;
using System.Net.Mail;
using Dede.Domain.Interfaces;
using Dede.Domain.Options;
using Microsoft.Extensions.Options;

namespace Dede.Service.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;

        public EmailSender(IOptions<SmtpSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.From),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(to);

            await client.SendMailAsync(message);
        }
    }
}