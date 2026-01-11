using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeatherStation.Api;
using WeatherStation.RemoteData.GeoApiCommunes;

namespace WeatherStation.RemoteData.IPGeolocation
{
    public class Ephemeris
    {
        #region Fields
        private City _city;
        private static DateOnly? _lastCallDate;
        #endregion

        #region Ctor
        public Ephemeris(City city)
        {
            _city = city;
        }
        #endregion

        /// <summary>
        /// Retrieves the daily ephemeris data for the specified city.
        /// This method checks if the ephemeris data has already been requested for the current day.
        /// If not, it calls the external API to fetch the latest ephemeris information (sun and moon data)
        /// for the city's coordinates, deserializes the JSON response into a <see cref="DailyEphemeris"/> object,
        /// and updates the last call date to prevent redundant requests within the same day.
        /// In case of an error during the API call or deserialization, the method logs the error to the console
        /// and returns <c>null</c>.
        /// </summary>
        /// <returns>
        /// A <see cref="DailyEphemeris"/> object containing the ephemeris data if the request is successful and
        /// the data has not already been fetched for the current day; otherwise, <c>null</c>.
        /// </returns>
        public async Task<DailyEphemeris?> LoadEphemerisDataAsync(RestClient clientApi, System.Net.HttpStatusCode?[] apiError)
        {
            if (true || _lastCallDate is null || _lastCallDate < DateOnly.FromDateTime(DateTime.Now))
            {
                try
                {
                    var json = await clientApi.GetEphemerisAsync(_city.Center.coordinates[1], _city.Center.coordinates[0]);
                    var response = JsonSerializer.Deserialize<DailyEphemeris>(json);
                    _lastCallDate = DateOnly.FromDateTime(DateTime.Now);
                    apiError[1] = null;
                    return response;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error fetching ephemeris data: {ex.Message}");
                    apiError[1] = ex.StatusCode;
                    _lastCallDate = null;
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error loading Ephemeris data: {ex.Message}");
                    apiError[1] = System.Net.HttpStatusCode.InternalServerError;
                    _lastCallDate = null;
                    throw;
                }
            }
            return null;
        }




        public class Location
        {
            [JsonPropertyName("latitude")]
            public string? Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public string? Longitude { get; set; }

            [JsonPropertyName("country_name")]
            public string? CountryName { get; set; }

            [JsonPropertyName("state_prov")]
            public string? StateProv { get; set; }

            [JsonPropertyName("city")]
            public string? City { get; set; }

            [JsonPropertyName("locality")]
            public string? Locality { get; set; }

            [JsonPropertyName("elevation")]
            public string? Elevation { get; set; }
        }

        public class DailyEphemeris
        {
            [JsonPropertyName("location")]
            public Location Location { get; set; } = new Location();

            [JsonPropertyName("date")]
            public string? Date { get; set; }

            [JsonPropertyName("current_time")]
            public string? CurrentTime { get; set; }

            [JsonPropertyName("sunrise")]
            public string? Sunrise { get; set; }

            [JsonPropertyName("sunset")]
            public string? Sunset { get; set; }

            [JsonPropertyName("sun_status")]
            public string? SunStatus { get; set; }

            [JsonPropertyName("solar_noon")]
            public string? SolarNoon { get; set; }

            [JsonPropertyName("day_length")]
            public string? DayLength { get; set; }

            [JsonPropertyName("sun_altitude")]
            public double? SunAltitude { get; set; }

            [JsonPropertyName("sun_distance")]
            public double? SunDistance { get; set; }

            [JsonPropertyName("sun_azimuth")]
            public double SunAzimuth { get; set; }

            [JsonPropertyName("moon_phase")]
            public string? MoonPhase { get; set; }

            [JsonPropertyName("moonrise")]
            public string? Moonrise { get; set; }

            [JsonPropertyName("moonset")]
            public string? Moonset { get; set; }

            [JsonPropertyName("moon_status")]
            public string? MoonStatus { get; set; }

            [JsonPropertyName("moon_altitude")]
            public double MoonAltitude { get; set; }

            [JsonPropertyName("moon_distance")]
            public double MoonDistance { get; set; }

            [JsonPropertyName("moon_azimuth")]
            public double MoonAzimuth { get; set; }

            [JsonPropertyName("moon_parallactic_angle")]
            public double MoonParallacticAngle { get; set; }

            [JsonPropertyName("moon_illumination_percentage")]
            public string? MoonIlluminationPercentage { get; set; }

            [JsonPropertyName("moon_angle")]
            public double MoonAngle { get; set; }
        }







    }
}