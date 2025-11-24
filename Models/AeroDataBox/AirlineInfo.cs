using System.Text.Json.Serialization;

namespace FlightInfoApi.Models.AeroDataBox
{
    public class AirlineInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("iata")]
        public string Iata { get; set; }

        [JsonPropertyName("icao")]
        public string Icao { get; set; }
    }
}
