using System.Text.Json.Serialization;

namespace FlightInfoApi.Models.AeroDataBox
{
    public class ArrivalInfo
    {
        [JsonPropertyName("airport")]
        public AirportInfo Airport { get; set; }

        [JsonPropertyName("scheduledTime")]
        public FlightTimestamp ScheduledTime { get; set; }

        [JsonPropertyName("revisedTime")]
        public FlightTimestamp RevisedTime { get; set; }

        [JsonPropertyName("terminal")]
        public string Terminal { get; set; }

        [JsonPropertyName("gate")]
        public string Gate { get; set; }

        [JsonPropertyName("quality")]
        public List<string> Quality { get; set; }
    }
}
