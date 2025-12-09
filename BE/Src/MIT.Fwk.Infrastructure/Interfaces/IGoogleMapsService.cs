using System.Threading.Tasks;
using static MIT.Fwk.Infrastructure.Services.GoogleMapsService;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    public interface IGoogleMapsService
    {
        public Task<MapsGeolocation> GeoLocalization();

        /// <summary>
        /// Trova tutti i posti di un certo tipo all'interno di una determinata area
        /// </summary>
        /// <param name="locations">Luoghi da cercare,inserire in un array come stringhe</param>
        /// <param name="count">Massimo numero di luoghi che restituirà il metodo (massimo 20 altrimenti le api di google ritornano un 404)</param>
        /// <param name="radius">Raggio in metri del cerchio entro cui cercherà</param>
        /// <param name="mapsLocation">Per specificare il punto da dove cerchare, se null il metodo eseguira automaticamente la geolocalizzazione</param>
        public Task<string> FindPlacesNearby(string[] locations, int count, double radius);

        public Task<string> FindPlacesByText(string languageCode, string searchText);
    }
}
