using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using MIT.Fwk.Infrastructure.Entities;
using System.Threading.Tasks;
using static MIT.Fwk.Infrastructure.Services.GoogleService;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    public interface IGoogleService
    {
        Task<string> GetGoogleCredentials(string code, bool forceRefreshToken = false, string userEmail = "", string redirectUri = "", string verifierCode = "");
        Task<string> GetAccessTokenByRefreshToken(string refreshToken);

        Task<GoogleInfos> GetGoogleInfos(Setup setup);

        Task<string> TryAddGoogleEvent(ThirdPartsToken tokens, string calendarName, Event newEvent, bool useRefreshToken = true);
        string CreateCalendarIfNotExists(CalendarService calendarService, string calendarName);
        Task<bool> TryRemoveGoogleEvent(ThirdPartsToken tokens, string eventId, string calendarName, bool useRefreshToken = true);
    }
}
