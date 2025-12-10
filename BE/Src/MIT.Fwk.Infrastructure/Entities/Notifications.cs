using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIT.Fwk.Infrastructure.Entities
{
    public class Notification : Identifiable<string>
    {
        [Attr] public string UserId { get; set; }
        [Attr] public string Email { get; set; }
        [Attr] public string Title { get; set; }
        [Attr] public string Body { get; set; }
        [Attr] public string Data { get; set; }
        [Attr] public bool OnlyData { get; set; }
        [Attr] public string MessageId { get; set; }
        [Attr] public DateTime? DateSent { get; set; }
        [Attr] public DateTime? DateRead { get; set; }
        [Attr] public bool Read { get; set; }
        [Attr] public bool Erased { get; set; }

        [Attr][NotMapped] public int TenantId { get; set; }
        [Attr][NotMapped] public string TemplateCode { get; set; }
        [Attr][NotMapped] public bool SendEmail { get; set; }
        [Attr][NotMapped] public bool SendPushNotification { get; set; }
        [Attr][NotMapped] public bool SendWebSocket { get; set; }
        [Attr][NotMapped] public bool ForceWebSocketApp { get; set; }
        [Attr] public string PushType { get; set; }
        [Attr][NotMapped] public string UsersGuidList { get; set; }

        [Attr][NotMapped] public string RolesGuidList { get; set; }
        [Attr][NotMapped] public string TenantsIdList { get; set; }

        [HasOne]
        public virtual User User { get; set; }
    }
}
