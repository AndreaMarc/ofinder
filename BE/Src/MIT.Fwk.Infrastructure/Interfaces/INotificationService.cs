using MIT.Fwk.Infrastructure.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    public interface INotificationService
    {
        Task<int> SendNotification(Notification resource, Setup setup, string baseEndpoint, Dictionary<string, string> customContent = null);
    }
}
