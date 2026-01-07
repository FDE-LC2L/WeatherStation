using System.Text.Json.Serialization;

namespace WeatherStation.RemoteData.GeoApiCommunes
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

        public static City? GetDefaultCity()
        {
            var json = """
                    
                        {
                        "nom": "Bayonne",
                        "code": "64102",
                        "codeDepartement": "64",
                        "siren": "216401026",
                        "codeEpci": "200067106",
                        "codeRegion": "75",
                        "codesPostaux": [
                            "64100"
                        ],
                        "population": 53312
                        }
                    
             """;
            var city = System.Text.Json.JsonSerializer.Deserialize<City>(json);
            city?.Center.type = "Point";
            city?.Center.coordinates.Add(-1.474301d);
            city?.Center.coordinates.Add(43.4844d);
            return city;


        }
    }
}
