using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MIT.Fwk.Core.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MIT.Fwk.Core.Services
{
    /// <summary>
    /// Implementation of IEmailService using MailKit SMTP.
    /// Provides email sending with SMTP configuration from SmtpOptions.
    /// </summary>
    public class EmailService : IEmailService
    {
        private const string LOG_CONTEXT = "mail-manager";

        private readonly SmtpOptions _smtpOptions;
        private readonly ILogService _logService;
        private readonly IEncryptionService _encryptionService;

        public event AsyncCompletedEventHandler SendCompleted;

        public EmailService(IOptions<SmtpOptions> smtpOptions, ILogService logService, IEncryptionService encryptionService)
        {
            _smtpOptions = smtpOptions?.Value ?? throw new ArgumentNullException(nameof(smtpOptions));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        protected virtual void OnSendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string token = (string)e.UserState;

            if (e.Cancelled)
            {
                _logService.Warn($"[{token}] Send canceled.", LOG_CONTEXT);
            }
            if (e.Error != null)
            {
                _logService.Error($"[{token}] Message error: {e.Error}", LOG_CONTEXT);
            }
            else
            {
                _logService.Info($"[{token}] Message sent.", LOG_CONTEXT);
            }

            SendCompleted?.Invoke(sender, e);
        }

        public IEnumerable<(string Recipient, bool Success, string ErrorMessage)> SendMail(
            string recipients,
            string body,
            string subject,
            List<System.Net.Mail.Attachment> attachments = null,
            string sender = "")
        {
            List<(string, bool, string)> results = new();

            IEnumerable<string> recs = recipients.Split(";").Distinct();

            if (!_smtpOptions.Enabled || !recs.Any())
            {
                return null;
            }

            // Decrypt credentials if encrypted
            string username = TryDecrypt(_smtpOptions.Username);
            string password = TryDecrypt(_smtpOptions.Password);

            foreach (string rec in recs)
            {
                if (string.IsNullOrEmpty(rec.Trim()))
                {
                    results.Add((rec, false, "Recipient is null or empty."));
                    continue;
                }

                string userToken = Guid.NewGuid().ToString();

                try
                {
                    // Create MimeMessage
                    MimeMessage message = new();

                    // Set From address
                    if (!string.IsNullOrEmpty(sender))
                    {
                        message.From.Add(new MailboxAddress(_smtpOptions.Sender, sender));
                        message.Sender = new MailboxAddress(_smtpOptions.Sender, sender);
                    }
                    else
                    {
                        message.From.Add(new MailboxAddress(_smtpOptions.Sender, username));
                    }

                    // Set To address
                    message.To.Add(MailboxAddress.Parse(rec));

                    // Set Subject
                    message.Subject = subject;

                    // Build body with attachments
                    BodyBuilder bodyBuilder = new()
                    {
                        HtmlBody = body
                    };

                    // Add attachments if present
                    if (attachments != null)
                    {
                        foreach (System.Net.Mail.Attachment att in attachments)
                        {
                            // Convert System.Net.Mail.Attachment to MimeKit attachment
                            bodyBuilder.Attachments.Add(att.Name, att.ContentStream);
                        }
                    }

                    message.Body = bodyBuilder.ToMessageBody();

                    _logService.Info($"[{userToken}] message recipient: {rec} subject: {subject}", LOG_CONTEXT);

                    // Send using MailKit SmtpClient
                    using SmtpClient client = new();

                    // Configure SSL certificate validation if needed
                    if (_smtpOptions.EnableSSL && _smtpOptions.IgnoreCertificateErrors)
                    {
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    }

                    try
                    {
                        // Connect to SMTP server
                        SecureSocketOptions secureOptions = _smtpOptions.EnableSSL
                            ? SecureSocketOptions.StartTls
                            : SecureSocketOptions.None;
                        client.Connect(_smtpOptions.Host, _smtpOptions.Port, secureOptions);

                        // Authenticate
                        client.Authenticate(username, password);

                        // Send message
                        client.Send(message);

                        // Disconnect
                        client.Disconnect(true);

                        // Trigger SendCompleted event with success
                        AsyncCompletedEventArgs eventArgs = new(null, false, userToken);
                        OnSendCompleted(client, eventArgs);

                        results.Add((rec, true, ""));
                    }
                    catch (Exception ex)
                    {
                        _logService.Error($"[{userToken}] message recipient: {rec} subject: {subject} err: {ex.Message}", LOG_CONTEXT);

                        // Trigger SendCompleted event with error
                        AsyncCompletedEventArgs eventArgs = new(ex, false, userToken);
                        OnSendCompleted(client, eventArgs);

                        results.Add((rec, false, $"Message: {ex.Message};\nInnerException:{ex.InnerException};"));
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error($"[{userToken}] message recipient: {rec} subject: {subject} err: {ex.Message}", LOG_CONTEXT);
                    results.Add((rec, false, $"Message: {ex.Message};\nInnerException:{ex.InnerException};"));
                    continue;
                }
            }

            return results;
        }

        /// <summary>
        /// Attempts to decrypt SMTP credentials using AES encryption key.
        /// If decryption fails (string not encrypted), returns original string.
        /// </summary>
        private string TryDecrypt(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            try
            {
                // Use default encryption key (same as DatabaseOptions)
                return _encryptionService.DecryptString(value, "2BB2AE87CABD4EFA8DE6CA723411CF7F");
            }
            catch
            {
                // Not encrypted or decryption failed - return as-is
                return value;
            }
        }
    }
}
