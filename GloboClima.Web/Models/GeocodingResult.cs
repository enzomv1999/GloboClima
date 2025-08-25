using System.Text.Json.Serialization;

namespace GloboClima.Web.Models
{
    public class GeocodingResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("latitude")]
        public double Lat { get; set; }

        [JsonPropertyName("longitude")]
        public double Lon { get; set; }
    }
}
