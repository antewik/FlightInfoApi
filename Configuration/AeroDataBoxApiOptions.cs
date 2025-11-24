namespace FlightInfoApi.Configuration
{
    /// <summary>
    /// Configuration options for AeroDataBox API access.
    /// Bound from appsettings.json.
    /// </summary>
    public class AeroDataBoxApiOptions
    {
        /// <summary>
        /// Base URL of the AeroDataBox API.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// API key used for authentication with AeroDataBox.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
    }
}
