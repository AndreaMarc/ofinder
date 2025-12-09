using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MIT.Fwk.Core.CQRS
{
    /// <summary>
    /// Domain notification for validation errors and business rule violations.
    /// Uses HTTP status codes for categorization.
    /// </summary>
    public class DomainNotification : Event
    {
        #region HTTP STATUS CODES

        // Serie 1xx - Informazioni
        public const string CONTINUE = "100";
        public const string SWITCHING_PROTOCOL = "101";
        public const string PROCESSING = "102";

        // Serie 2xx - L'azione è stata ricevuta con successo, compresa ed accettata.
        public const string OK = "200";
        public const string CREATED = "201";
        public const string ACCEPTED = "202";
        public const string NON_AUTHORITATIVE_INFORMATION = "203";
        public const string NO_CONTENT = "204";
        public const string RESET_CONTENT = "205";
        public const string PARTIAL_CONTENT = "206";
        public const string MULTI_STATUS = "207";
        public const string ALREADY_REPORTED = "208";

        // Serie 3xx - Il client deve eseguire ulteriori azioni per soddisfare la richiesta.
        public const string MULTIPLE_CHOICES = "300";
        public const string MOVED_PERMANENTLY = "301";
        public const string FOUND = "302";
        public const string SEE_OTHER = "303";
        public const string NOT_MODIFIED = "304";
        public const string USE_PROXY = "305";
        public const string SWITCH_PROXY = "306";
        public const string TEMPORARY_REDIRECT = "307";
        public const string PERMANENT_REDIRECT = "308";

        // Serie 4xx - La richiesta è sintatticamente scorretta o non può essere soddisfatta.
        public const string BAD_REQUEST = "400";
        public const string UNAUTHORIZED = "401";
        public const string UNAUTHORIZED_FIRST_ACCESS = "401.1";
        public const string UNAUTHORIZED_PASSWORD_EXPIRED = "401.2";
        public const string UNAUTHORIZED_LOCKED = "401.3";
        public const string PAYMENT_REQUIRED = "402";
        public const string FORBIDDEN = "403";
        public const string NOT_FOUND = "404";
        public const string METHOD_NOT_ALLOWED = "405";
        public const string NOT_ACCEPTABLE = "406";
        public const string PROXY_AUTHENTICATION_REQUIRED = "407";
        public const string REQUEST_TIMEOUT = "408";
        public const string CONFLICT = "409";
        public const string GONE = "410";
        public const string LENGTH_REQUIRED = "411";
        public const string PRECONDITION_FAILED = "412";
        public const string REQUEST_ENTITY_TOO_LARGE = "413";
        public const string REQUEST_URI_TOO_LONG = "414";
        public const string UNSUPPORTED_MEDIA_TYPE = "415";
        public const string REQUEST_RANGE_NOT_SATISFIABLE = "416";
        public const string EXPECTATION_FAILED = "417";
        public const string IM_A_TEAPOT = "418";
        public const string ENHANCE_YOUR_CALM = "420";
        public const string UNPROCESSABLE_ENTITY = "422";
        public const string LOCKED = "423";
        public const string UNPROCESSABLE_ENTITY_MISSING_DATA = "422.1";
        public const string UPGRADE_REQUIRED = "426";
        public const string TOO_MANY_REQUESTS = "429";
        public const string RETRY_WITH = "449";
        public const string UNAVILABLE_FOR_LEGAL_REASONS = "451";

        // Serie 5xx - Il server ha fallito nel soddisfare una richiesta apparentemente valida.
        public const string INTERNAL_SERVER_ERROR = "500";
        public const string NOT_IMPLEMENTED = "501";
        public const string BAD_GATEWAY = "502";
        public const string SERVICE_UNAVAILABLE = "503";
        public const string GATEWAY_TIMEOUT = "504";
        public const string HTTP_VERSION_NOT_SUPPORTED = "505";
        public const string BANDWIDTH_LIMIT_EXCEEDED = "509";

        #endregion

        public Guid DomainNotificationId { get; private set; }
        public string Code { get; private set; }
        public string Description { get; private set; }
        public int Version { get; private set; }

        // XXXX.YY
        // XXXX HTTP STATUS CODE
        // YY DETAIL CODE
        public int HttpStatus
        {
            get
            {
                try
                {
                    return int.Parse(Code.Split('.')[0]);
                }
                catch
                {
                    return 500;
                }
            }
        }

        public DomainNotification(string code, string description)
        {
            DomainNotificationId = Guid.NewGuid();
            Version = 1;
            Code = code;
            Description = JsonConvert.SerializeObject(new NotificationMessage() { Code = code, Description = description });
        }

        class NotificationMessage
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }
    }

    /// <summary>
    /// Handler for collecting and managing domain notifications.
    /// Implements MediatR's INotificationHandler.
    /// </summary>
    public class DomainNotificationHandler : INotificationHandler<DomainNotification>
    {
        private List<DomainNotification> _notifications;

        public DomainNotificationHandler()
        {
            _notifications = [];
        }

        public Task Handle(DomainNotification message, CancellationToken cancellationToken = default)
        {
            _notifications.Add(message);
            return Task.CompletedTask;
        }

        public virtual List<DomainNotification> GetNotifications()
        {
            return _notifications;
        }

        public virtual bool HasNotifications()
        {
            return GetNotifications().Any();
        }

        public void Dispose()
        {
            _notifications = [];
        }
    }
}
