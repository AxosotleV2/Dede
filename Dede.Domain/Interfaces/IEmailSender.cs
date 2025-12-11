// Dede.Domain/Interfaces/IEmailSender.cs
namespace Dede.Domain.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }
}