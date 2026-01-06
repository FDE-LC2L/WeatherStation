using System.Net.Http;

namespace WeatherStation.Api
{
    public class RestApiClient
    {
        #region Fields
        //const string InfoClimatApiKeyTst = "AxkCFVIsUHJWe1BnAHYAKVQ8BjMIflRzAX0HZAtuVyoAa1c2AGBRN1A%2BWyZTfAcxVntTMAA7VWVWPQB4D30DYgNpAm5SOVA3VjlQNQAvACtUegZnCChUcwFjB2gLY1cqAGJXOwBmUS1QN1snU2IHNVZhUywAIFVsVjIAYQ9lA2YDZAJkUjhQMlYwUC0ALwAyVG4GYwhkVG0BZQdhCzJXPAA3V2IAalE3UDZbJ1NjBztWY1M6ADhVZFYyAG4PfQN%2FAxkCFVIsUHJWe1BnAHYAKVQyBjgIYw%3D%3D&_c=75c1eb39c62a66a007cf0c09bfaa2057";
        //const string InfoClimatApiUrlTst = $"http://www.infoclimat.fr/public-api/gfs/json?_ll=43.863485,-0.702268&_auth={InfoClimatApiKey}";


        const string InfoClimatApiKey = "AhgHEFYoBiRVeFNkBHIHLgRsATQPeVN0US1RMl86An8FbgJjB2dVM1U7USxXeAA2UH1XNFxnUGAAa1UtDH5TMgJoB2tWPQZhVTpTNgQrBywEKgFgDy9TdFEzUT5fNwJ%2FBWcCbgdhVSlVMlEtV2YAMlBnVyhcfFBpAGRVNgxkUzQCYgdjVjQGZFU7Uy4EKwc1BD8BYQ85U2lRNFEzXzcCMgVvAmcHbFUwVT9RLVdlADNQalc1XGFQaQBrVTcMflMvAhgHEFYoBiRVeFNkBHIHLgRiAT8PZA%3D%3D&_c=cb6a40c35f2808f273a58f02d80958ad";        
        const string InfoClimatApiUrl = "http://www.infoclimat.fr/public-api/gfs/json?_ll={0},{1}&_auth=" + InfoClimatApiKey;

        const string GeoApiCommunes = "https://geo.api.gouv.fr/communes";

        private readonly HttpClient _httpClient;
        #endregion

        public RestApiClient()
        {
            _httpClient = new HttpClient();
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
            var url = string.Format(InfoClimatApiUrl, latitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture), longitude.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
           // url = InfoClimatApiUrlTst;
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetInfoCommunesAsync(string? postalCode, string? cityName = null)
        {
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


    }
}
