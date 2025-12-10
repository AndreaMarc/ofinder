using Microsoft.Extensions.Configuration;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static MIT.Fwk.Infrastructure.Services.GoogleService;

namespace MIT.Fwk.Infrastructure.Services
{
    //questo serve per fare operazioni di crud su entita che hanno controller jsonapi, utilizzando il dbcontext
    //ad esempio se sto su un controller dell'entita A e faccio un override, con questo posso fare crud anche sull'entita B
    public class GoogleMapsService : IGoogleMapsService
    {
        private readonly JsonApiDbContext _context;
        private readonly IJsonApiManualService _manualService;
        private readonly IConfiguration _configuration;
        private readonly GoogleInfos googleInfos;

        public GoogleMapsService(JsonApiDbContext context, IJsonApiManualService jsonApiManualService, IConfiguration configuration)
        {
            _context = context;
            _manualService = jsonApiManualService;
            _configuration = configuration;
            googleInfos = GetGoogleInfos();
        }

        public GoogleInfos GetGoogleInfos()
        {
            Setup webSetup = _manualService.FirstOrDefault<Setup, int>(x => x.environment == "web");
            return JsonConvert.DeserializeObject<GoogleInfos>(webSetup.googleCredentials);
        }

        public static string CreateSearchParams(string[] locations, int count, double radius, MapsLocation mapslocation)
        {
            GeoLocation location = new()
            {
                Latitude = mapslocation.Lat,
                Longitude = mapslocation.Lng,
            };
            Circle cicle = new()
            {
                Radius = radius,
                Center = location
            };

            LocationRestriction locationRestriction = new()
            {
                Circle = cicle
            };

            SearchParams searchParams = new()
            {
                IncludedTypes = locations,
                MaxResultCount = count,
                LocationRestriction = locationRestriction
            };
            return JsonConvert.SerializeObject(searchParams);
        }

        public async Task<MapsGeolocation> GeoLocalization()
        {
            using HttpClient client = new();
            try
            {
                StringContent content = new(string.Empty, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(
                    "https://www.googleapis.com/geolocation/v1/geolocate?key=" + googleInfos.GoogleAPIKey,
                    content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<MapsGeolocation>(responseData);
                }
                else
                {
                    return null;
                }
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<string> FindPlacesNearby(string[] locations, int count, double radius)
        {
            IConfigurationSection configuration = _configuration.GetSection("GoogleMapsFields:SearchNearby");

            string fields = configuration.Exists()
                ? configuration.Value
                : "places.id,places.displayName.text,places.location,places.formattedAddress,places.addressComponents";
            MapsLocation mapsLocation = (await GeoLocalization())?.Location;

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("X-Goog-Api-Key", googleInfos.GoogleAPIKey);
            client.DefaultRequestHeaders.Add("X-Goog-FieldMask", fields);

            string jsonData = CreateSearchParams(locations, count, radius, mapsLocation);
            StringContent content = new(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("https://places.googleapis.com/v1/places:searchNearby", content);

                return response.StatusCode == HttpStatusCode.OK
                    ? await response.Content.ReadAsStringAsync()
                    : JsonConvert.SerializeObject(new object { });
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<string> FindPlacesByText(string languageCode, string searchText)
        {
            IConfigurationSection configuration = _configuration.GetSection("GoogleMapsFields:SearchText");

            string fields = configuration.Exists()
                ? configuration.Value
                : "places.id,places.displayName.text,places.location,places.formattedAddress,places.addressComponents";
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("X-Goog-Api-Key", googleInfos.GoogleAPIKey);
            client.DefaultRequestHeaders.Add("X-Goog-FieldMask", fields);
            client.DefaultRequestHeaders.Add("Accept-Language", languageCode);

            string jsonData = JsonConvert.SerializeObject(new { textQuery = searchText });
            StringContent content = new(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("https://places.googleapis.com/v1/places:searchText", content);

                return response.StatusCode == HttpStatusCode.OK
                    ? await response.Content.ReadAsStringAsync()
                    : JsonConvert.SerializeObject(new object { });
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }


        #region geolocalization
        public class MapsGeolocation
        {
            public MapsLocation Location { get; set; }
        }
        public class MapsLocation
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
        #endregion

        #region body params
        private class SearchParams
        {
            public string[] IncludedTypes { get; set; }
            public int MaxResultCount { get; set; }
            public LocationRestriction LocationRestriction { get; set; }

            public string LanguageCode { get; init; } = "it";
        }

        private class LocationRestriction
        {
            public Circle Circle { get; set; }
        }

        private class Circle
        {
            public GeoLocation Center { get; set; }
            public double Radius { get; set; }
        }

        public class GeoLocation
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
        #endregion

    }
}
