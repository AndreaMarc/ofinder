using Microsoft.Extensions.Logging;
using MIT.Fwk.Core.Services;
using MIT.Fwk.Infrastructure.Interfaces;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Handlers
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly ILogger<AuthMessageSender> _logger;
        private readonly IEmailService _emailService;

        public AuthMessageSender(ILogger<AuthMessageSender> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public Task<IEnumerable<(string Recipient, bool Success, string ErrorMessage)>> SendEmailAsync(string email, string subject, string message, List<Attachment> attachments = null, string sender = "")
        {
            IEnumerable<(string, bool, string)> sendResults = _emailService.SendMail(email, message, subject, attachments, sender);

            _logger.LogInformation("Email sent - Email: {Email}, Subject: {Subject}", email, subject);
            return Task.FromResult(sendResults);
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            _logger.LogInformation("SMS sent - Number: {Number}, Message: {Message}", number, message);
            return Task.FromResult(0);
        }
    }
}
