using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    public interface IEmailSender
    {
        Task<IEnumerable<(string Recipient, bool Success, string ErrorMessage)>> SendEmailAsync(string email, string subject, string message, List<Attachment> attachments = null, string sender = "");
    }
}
