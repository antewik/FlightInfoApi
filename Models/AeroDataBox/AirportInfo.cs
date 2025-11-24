using System.Text.Json.Serialization;

namespace FlightInfoApi.Models.AeroDataBox
{
    public class AirportInfo
    {
        [JsonPropertyName("icao")]
        public string Icao { get; set; }

        [JsonPropertyName("iata")]
        public string Iata { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("timeZone")]
        public string TimeZone { get; set; }
    }
}