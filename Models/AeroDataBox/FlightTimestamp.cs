using FlightInfoApi.Converters;
using System.Text.Json.Serialization;

namespace FlightInfoApi.Models.AeroDataBox
{
    public class FlightTimestamp
    {
        [JsonPropertyName("utc")]
        [JsonConverter(typeof(AeroDateTimeOffsetConverter))]
        public DateTimeOffset? Utc { get; set; }

        [JsonPropertyName("local")]
        [JsonConverter(typeof(AeroDateTimeOffsetConverter))]
        public DateTimeOffset? Local { get; set; }
    }
}