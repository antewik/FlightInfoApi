using System.Text.Json.Serialization;

namespace FlightInfoApi.Models.AeroDataBox
{
    public class AircraftInfo
    {
        [JsonPropertyName("reg")]
        public string Reg { get; set; }

        [JsonPropertyName("modeS")]
        public string ModeS { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }
    }
}
