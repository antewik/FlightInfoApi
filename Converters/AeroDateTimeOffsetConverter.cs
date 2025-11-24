using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlightInfoApi.Converters
{
    /// <summary>
    /// Custom JSON converter for DateTimeOffset?.
    /// Handles AeroDataBox API quirks where timestamps may use a space instead of 'T'.
    /// </summary>
    public class AeroDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
    {
        /// <summary>
        /// Read and parse a DateTimeOffset? value from JSON.
        /// Accepts both standard ISO 8601 and space-separated formats.
        /// </summary>
        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var raw = reader.GetString();
            if (string.IsNullOrWhiteSpace(raw)) return null;

            // Try parsing with space instead of T (API sometimes returns "2025-11-22 10:30:00+00:00")
            if (DateTimeOffset.TryParse(raw.Replace(" ", "T"), out var dto))
            {
                return dto;
            }

            // Throw if parsing fails
            throw new JsonException($"Invalid DateTimeOffset format: {raw}");
        }

        /// <summary>
        /// Write a DateTimeOffset? value to JSON in ISO 8601 format.
        /// Example: "2025-11-22T10:30:00+00:00"
        /// </summary>
        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString("yyyy-MM-ddTHH:mm:sszzz"));
        }
    }
}
