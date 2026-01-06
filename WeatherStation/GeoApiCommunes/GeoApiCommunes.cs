using System.Text.Json.Serialization;

namespace WeatherStation.Geo
{
    public class CityCenter
    {
        public string type { get; set; } = string.Empty;
        public List<double> coordinates { get; set; } = new List<double>();
    }


    public class City
    {
        [JsonPropertyName("nom")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("codeDepartement")]
        public string DepartmentCode { get; set; } = string.Empty;

        [JsonPropertyName("siren")]
        public string Siren { get; set; } = string.Empty;

        [JsonPropertyName("codeEpci")]
        public string EpciCode { get; set; } = string.Empty;

        [JsonPropertyName("codeRegion")]
        public string RegionCode { get; set; } = string.Empty;

        [JsonPropertyName("codesPostaux")]
        public List<string> PostalCodes { get; set; } = new List<string>();

        [JsonPropertyName("population")]
        public int Population { get; set; }

        [JsonPropertyName("centre")]
        public CityCenter Center { get; set; } = new CityCenter { };

        [JsonIgnore]
        public string FormattedName { get => $"{Name} {PostalCodes[0]}"; }

    }
}
