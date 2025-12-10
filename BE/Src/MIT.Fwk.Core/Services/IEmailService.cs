using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Mail;

namespace MIT.Fwk.Core.Services
{
    /// <summary>
    /// Email sending service abstraction.
    /// Provides SMTP-based email delivery with attachment support.
    /// Replaces static MailHelper with dependency injection pattern.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Event fired when email sending completes (success or failure).
        /// </summary>
        event AsyncCompletedEventHandler SendCompleted;

        /// <summary>
        /// Sends an email to one or more recipients.
        /// </summary>
        /// <param name="recipients">Semicolon-separated list of recipient email addresses</param>
        /// <param name="body">Email body (HTML supported)</param>
        /// <param name="subject">Email subject</param>
        /// <param name="attachments">Optional list of attachments</param>
        /// <param name="sender">Optional sender email (overrides default SMTP username)</param>
        /// <returns>Collection of results per recipient (email, success, error message)</returns>
        IEnumerable<(string Recipient, bool Success, string ErrorMessage)> SendMail(
            string recipients,
            string body,
            string subject,
            List<Attachment> attachments = null,
            string sender = "");
    }
}
