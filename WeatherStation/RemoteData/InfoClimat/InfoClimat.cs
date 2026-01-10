using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeatherStation.Api;
using WeatherStation.RemoteData.GeoApiCommunes;


namespace WeatherStation.WeatherData.InfoClimat
{

    public class InfoClimatManager
    {
        public enum Hour
        {
            Zero = 0,
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6,
            Seven = 7,
            Eight = 8,
            Nine = 9,
            Ten = 10,
            Eleven = 11,
            Twelve = 12,
            Thirteen = 13,
            Fourteen = 14,
            Fifteen = 15,
            Sixteen = 16,
            Seventeen = 17,
            Eighteen = 18,
            Nineteen = 19,
            Twenty = 20,
            TwentyOne = 21,
            TwentyTwo = 22,
            TwentyThree = 23,
        }

        #region Fields
        private WeatherResponse? _weatherResponse;
        private readonly City _city;
        public WeatherResponse? WeatherResponse { get => _weatherResponse; }

        #endregion

        #region Ctor
        public InfoClimatManager(City city)
        {
            _city = city;
        }
        #endregion

        /// <summary>
        /// Asynchronously loads weather data from the InfoClimat API and deserializes it into a <see cref="WeatherResponse"/> object.
        /// If the deserialized response contains valid forecast data, it is stored in the private <c>_weatherResponse</c> field.
        /// </summary>
        /// <remarks>
        /// This method uses the <see cref="RestApiClient"/> to retrieve weather data in JSON format.
        /// The JSON is then deserialized into a <see cref="WeatherResponse"/> instance.
        /// If the response contains forecasts, the internal state is updated accordingly.
        /// </remarks>
        public async Task LoadInfoClimatDataAsync(HttpStatusCode?[] apiError)
        {
            try
            {
                var apiClient = new RestApiClient();
                var json = await apiClient.GetInfoClimatWheatherDataAsync(_city.Center.coordinates[1], _city.Center.coordinates[0]);
                var response = JsonSerializer.Deserialize<WeatherResponse>(json);
                if (response?.Forecasts is object)
                {
                    _weatherResponse = response;
                }
                apiError[0] = null; // Indicate success
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error loading InfoClimat data: {ex.Message}");
                apiError[0] = ex.StatusCode; // Indicate an error occurred
            }
        }

        /// <summary>
        /// Retrieves the weather forecast for a specific date and time, using the date string as a key.
        /// </summary>
        /// <param name="date">
        /// The date and time string in the format "yyyy-MM-dd HH:mm:ss" corresponding to the forecast to retrieve.
        /// </param>
        /// <returns>
        /// The <see cref="ForecastData"/> object for the specified date and time if available; otherwise, <c>null</c>.
        /// </returns>
        public ForecastData? GetForecast(string date)
        {
            if (_weatherResponse?.Forecasts is object && _weatherResponse.Forecasts.TryGetValue(date, out ForecastData? value))
            {
                value.DateString = date;
                return value;
            }
            return null;
        }

        /// <summary>
        /// Retrieves the weather forecast for a specific date and hour.
        /// </summary>
        /// <param name="date">
        /// The <see cref="DateOnly"/> representing the date for which the forecast is requested.
        /// </param>
        /// <param name="hour">
        /// The <see cref="Hour"/> enumeration value representing the hour of the day (default is 14:00).
        /// </param>
        /// <returns>
        /// The <see cref="ForecastData"/> object for the specified date and hour if available; otherwise, <c>null</c>.
        /// </returns>
        public ForecastData? GetForecast(DateOnly date, Hour hour = Hour.Fourteen)
        {
            var hourString = ((int)hour).ToString("00") + ":00:00";
            var dateString = date.ToString("yyyy-MM-dd ") + hourString;
            var v = GetForecast(dateString);
            return v;
        }

        /// <summary>
        /// Retrieves all weather forecasts available for a specific day.
        /// </summary>
        /// <param name="date">
        /// The <see cref="DateOnly"/> representing the day for which forecasts are requested.
        /// </param>
        /// <returns>
        /// A dictionary mapping date-time strings (formatted as "yyyy-MM-dd HH:mm:ss") to <see cref="ForecastData"/> objects for the specified day,
        /// or <c>null</c> if no forecasts are available.
        /// </returns>
        public List<ForecastData?>? GetForecastsForDay(DateOnly date)
        {
            var forecasts = _weatherResponse?.Forecasts?.Where(f => f.Key.StartsWith(date.ToString("yyyy-MM-dd")));
            if (forecasts?.Count() > 0)
            {
                foreach (var f in forecasts)
                {
                    f.Value.DateString = f.Key;
                }
            }
            //return _weatherResponse?.Forecasts?.Where(f => f.Key.StartsWith(date.ToString("yyyy-MM-dd"))).ToDictionary(f => f.Key, f => f.Value);             
            return forecasts?.Select(f => f.Value).ToList<ForecastData?>();
        }

        /// <summary>
        /// Returns the number of weather forecasts available for a given day.
        /// </summary>
        /// <param name="date">
        /// The <see cref="DateOnly"/> representing the day for which the forecast count is requested.
        /// </param>
        /// <returns>
        /// The number of forecasts available for the specified day, or 0 if no forecasts are present.
        /// </returns>
        public int GetCountForecastForDay(DateOnly date)
        {
            return _weatherResponse?.Forecasts?.Count(f => f.Key.StartsWith(date.ToString("yyyy-MM-dd"))) ?? 0;
        }
    }


    public class WeatherResponse
    {
        [JsonPropertyName("request_state")]
        public int RequestState { get; set; }

        [JsonPropertyName("request_key")]
        public string RequestKey { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("model_run")]
        public string ModelRun { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonExtensionData]
        private Dictionary<string, JsonElement>? _extensionData;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData
        {
            get => _extensionData;
            set
            {
                _extensionData = value;
                if (_extensionData is object)
                {
                    foreach (var kvp in _extensionData)
                    {
                        if (DateTime.TryParse(kvp.Key, out _))
                        {
                            var forecast = kvp.Value.Deserialize<ForecastData>();
                            if (forecast is object && Forecasts is object)
                            {
                                Forecasts[kvp.Key] = forecast;
                            }
                        }
                    }
                }
            }
        }

        [JsonIgnore]
        public Dictionary<string, ForecastData>? Forecasts { get => GetForecasts(); }

        private Dictionary<string, ForecastData>? GetForecasts()
        {
            if (ExtensionData is object)
            {
                var forecasts = ExtensionData
                    .Where(kvp => DateTime.TryParse(kvp.Key, out _)) // Filtrer uniquement les clés valides
                    .Select(kvp => new
                    {
                        kvp.Key,
                        Value = kvp.Value.Deserialize<ForecastData>()
                    })
                    .Where(x => x.Value != null) // Filtrer les valeurs nulles après désérialisation
                    .ToDictionary(x => x.Key, x => x.Value!);
                return forecasts;
            }
            return null;
        }
    }

    // You can deserialize RawForecasts values into this class manually
    public class ForecastData
    {
        [JsonPropertyName("temperature")]
        public Temperature? Temperature { get; set; }

        [JsonPropertyName("pression")]
        public Pressure? Pressure { get; set; }

        [JsonPropertyName("pluie")]
        public double Rain { get; set; }

        [JsonPropertyName("pluie_convective")]
        public double ConvectiveRain { get; set; }

        [JsonPropertyName("humidite")]
        public Humidity? Humidity { get; set; }

        [JsonPropertyName("vent_moyen")]
        public Wind? WindAverage { get; set; }

        [JsonPropertyName("vent_rafales")]
        public Wind? WindGusts { get; set; }

        [JsonPropertyName("vent_direction")]
        public Wind? WindDirection { get; set; }

        [JsonPropertyName("iso_zero")]
        public int FreezingLevel { get; set; }

        [JsonPropertyName("risque_neige")]
        public string SnowRisk { get; set; } = string.Empty;

        [JsonPropertyName("cape")]
        public int Cape { get; set; }

        [JsonPropertyName("nebulosite")]
        public Cloudiness? Cloudiness { get; set; }

        [JsonIgnore]
        public string DateString { get; set; } = string.Empty;
    }

    public class Temperature
    {
        [JsonPropertyName("2m")]
        public double At2m { get; set; }

        [JsonPropertyName("sol")]
        public double Ground { get; set; }

        [JsonPropertyName("500hPa")]
        public double At500hPa { get; set; }

        [JsonPropertyName("850hPa")]
        public double At850hPa { get; set; }
    }

    public class Pressure
    {
        [JsonPropertyName("niveau_de_la_mer")]
        public int SeaLevel { get; set; }
    }

    public class Humidity
    {
        [JsonPropertyName("2m")]
        public double At2m { get; set; }
    }

    public class Wind
    {
        [JsonPropertyName("10m")]
        public double At10m { get; set; }
    }

    public class Cloudiness
    {
        [JsonPropertyName("haute")]
        public int High { get; set; }

        [JsonPropertyName("moyenne")]
        public int Medium { get; set; }

        [JsonPropertyName("basse")]
        public int Low { get; set; }

        [JsonPropertyName("totale")]
        public int Total { get; set; }
    }

}
