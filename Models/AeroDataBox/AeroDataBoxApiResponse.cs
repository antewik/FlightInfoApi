using System.Text.Json.Serialization;

namespace FlightInfoApi.Models.AeroDataBox
{
    public class AeroDataBoxApiResponse
    {
        [JsonPropertyName("departures")]
        public List<FlightRecord> Departures { get; set; }


        [JsonPropertyName("arrivals")]
        public List<FlightRecord> Arrivals { get; set; }
    }
}
