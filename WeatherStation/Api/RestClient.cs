using System.Net.Http;
using WeatherStation.Infrastructure;

namespace WeatherStation.Api
{
    public class RestClient
    {
        #region Fields
        private readonly HttpClient _httpClient;
        private readonly AppSettingsManager appSettingsManager = AppSettingsManager.Instance;
        #endregion

        public RestClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Sends an HTTP GET request to the "/api/status" endpoint and returns the response content as a string.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{String}"/> representing the asynchronous operation, containing the response body as a string.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP response indicates an unsuccessful status code.
        /// </exception>
        public async Task<string> GetStatusAsync()
        {
            var response = await _httpClient.GetAsync("/api/status");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Sends an HTTP GET request to the InfoClimat API endpoint and returns the weather data as a string.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{String}"/> representing the asynchronous operation, containing the weather data in JSON format as a string.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP response indicates an unsuccessful status code.
        /// </exception>
        public async Task<string> GetInfoClimatWheatherDataAsync(double latitude, double longitude)
        {
            const string apiUrl = "http://www.infoclimat.fr/public-api/gfs/json?_ll={0},{1}&_auth={2}";
            var url = string.Format(apiUrl, latitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture), longitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture), appSettingsManager.InfoClimatApiKey);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Sends an HTTP GET request to the French governmental Geo API to retrieve information about communes.
        /// </summary>
        /// <param name="postalCode">
        /// The postal code of the commune to search for. If null or empty, this parameter is ignored.
        /// </param>
        /// <param name="cityName">
        /// The name of the commune to search for. This parameter is optional and can be null.
        /// </param>
        /// <returns>
        /// A <see cref="Task{String}"/> representing the asynchronous operation, containing the response body in JSON format as a string.
        /// </returns>        
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP response indicates an unsuccessful status code.
        /// </exception>
        public async Task<string> GetInfoCommunesAsync(string? postalCode, string? cityName = null)
        {
            const string GeoApiCommunes = "https://geo.api.gouv.fr/communes";
            var parameters = $"fields=nom,code,codesPostaux,centre,contour,population&format=json";
            if (!string.IsNullOrEmpty(postalCode))
            {
                parameters += $"&codePostal={postalCode}";
            }
            if (!string.IsNullOrEmpty(cityName))
            {
                parameters += $"&nom={cityName}";
            }
            var response = await _httpClient.GetAsync($"{GeoApiCommunes}?{parameters}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Sends an asynchronous HTTP GET request to the IPGeolocation Astronomy API to retrieve ephemeris data (astronomical information such as sunrise, sunset, etc.) for the specified geographic coordinates.
        /// </summary>
        /// <param name="latitude">
        /// The latitude of the location for which ephemeris data is requested.
        /// </param>
        /// <param name="longitude">
        /// The longitude of the location for which ephemeris data is requested.
        /// </param>
        /// <returns>
        /// A <see cref="Task{String}"/> representing the asynchronous operation, containing the ephemeris data in JSON format as a string.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP response indicates an unsuccessful status code.
        /// </exception>
        public async Task<string> GetEphemerisAsync(double latitude, double longitude)
        {
            const string apiUrl = "https://api.ipgeolocation.io/astronomy?apiKey={0}&lat={1}&long={2}";
            var requestUrl = string.Format(apiUrl, appSettingsManager.IpGeoLocationApiKey, latitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture), longitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Sends an asynchronous HTTP GET request to the Nominis API to retrieve the saint of the day for a given date.
        /// </summary>
        /// <param name="dateOnly">
        /// The date for which the saint of the day is requested.
        /// </param>
        /// <returns>
        /// A <see cref="Task{String}"/> representing the asynchronous operation, containing the response body in JSON format as a string.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when the HTTP response indicates an unsuccessful status code.
        /// </exception>
        public async Task<string> GetNominisAsync(DateOnly dateOnly)
        {
            const string apiUrl = "https://nominis.cef.fr/json/saintdujour.php?jour={0}&mois={1}&annee={2}";
            var requestUrl = string.Format(apiUrl, dateOnly.Day.ToString("D2"), dateOnly.Month.ToString("D2"), dateOnly.Year.ToString("D4"));
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
