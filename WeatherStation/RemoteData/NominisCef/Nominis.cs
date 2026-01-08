using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeatherStation.Api;

namespace WeatherStation.RemoteData.NominisCef
{
    public class Nominis
    {

        #region Fields
        private static DateOnly? _lastCallDate;
        #endregion

        public static async Task<DailyNominis?> LoadNominisDataAsync(HttpStatusCode?[] apiError)
        {
            if (_lastCallDate is null || _lastCallDate < DateOnly.FromDateTime(DateTime.Now))
            {
                try
                {
                    var apiClient = new RestApiClient();
                    var json = await apiClient.GetNominisAsync(DateOnly.FromDateTime(DateTime.Now));
                    var response = JsonSerializer.Deserialize<DailyNominis>(json);
                    _lastCallDate = DateOnly.FromDateTime(DateTime.Now);
                    apiError[2] = null;
                    return response;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error fetching nominis data: {ex.Message}");
                    apiError[2] = ex.StatusCode;
                    _lastCallDate = null;
                    return null;
                }
            }
            return null;
        }

    }


    public class DailyNominis
    {
        [JsonPropertyName("response")]
        public Response? Response { get; set; }
    }


    public class Query
    {
        [JsonPropertyName("jour")]
        public string? Day { get; set; }

        [JsonPropertyName("mois")]
        public string? Month { get; set; }

        [JsonPropertyName("annee")]
        public string? Year { get; set; }
    }

    public class Response
    {
        [JsonPropertyName("query")]
        public Query? Query { get; set; } = new Query();

        [JsonPropertyName("saintdujour")]
        public SaintOfTheDay? SaintOfTheDay { get; set; } = new SaintOfTheDay();
    }

    public class SaintOfTheDay
    {
        [JsonPropertyName("nom")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("lien")]
        public string? Link { get; set; }

        [JsonPropertyName("contenu")]
        public string? Content { get; set; }

        [JsonPropertyName("copyright")]
        public string? Copyright { get; set; }

        [JsonPropertyName("modification")]
        public string? Modification { get; set; }

        [JsonPropertyName("audio")]
        public string? Audio { get; set; }
    }
}
