namespace FlightInfoApi.Configuration
{
    /// <summary>
    /// Configuration options for mock flight data.
    /// Used to control whether the service returns mock flights instead of real API data.
    /// Bound from appsettings.json.
    /// </summary>
    public class MockOptions
    {
        /// <summary>
        /// Flag to enable mock mode.
        /// If true, the service will generate mock flights instead of calling AeroDataBox.
        /// </summary>
        public bool UseMockData { get; set; } = false;

        /// <summary>
        /// Number of mock flights to generate when mock mode is enabled.
        /// </summary>
        public int MockFlightCount { get; set; } = 50;
    }
}
