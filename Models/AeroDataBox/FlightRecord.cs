using System.Text.Json.Serialization;

namespace FlightInfoApi.Models.AeroDataBox
{
    public class FlightRecord
    {
        [JsonPropertyName("movement")]
        public MovementInfo Movement { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("callSign")]
        public string CallSign { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("codeshareStatus")]
        public string CodeshareStatus { get; set; }

        [JsonPropertyName("isCargo")]
        public bool IsCargo { get; set; }

        [JsonPropertyName("aircraft")]
        public AircraftInfo Aircraft { get; set; }

        [JsonPropertyName("airline")]
        public AirlineInfo Airline { get; set; }
    }
}
