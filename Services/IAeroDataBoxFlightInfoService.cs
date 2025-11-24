using FlightInfoApi.Models;

namespace FlightInfoApi.Services
{
    /// <summary>
    /// Contract for retrieving flight information from AeroDataBox.
    /// Provides methods for departures and arrivals.
    /// </summary>
    public interface IAeroDataBoxFlightInfoService
    {
        /// <summary>
        /// Get departures for a given airport within the specified time window.
        /// </summary>
        /// <param name="iataCode">The IATA airport code (e.g., "LAX").</param>
        /// <param name="offsetMinutes">Minutes offset from current time to start the window.</param>
        /// <param name="durationMinutes">Length of the time window in minutes.</param>
        /// <returns>A list of flight information records.</returns>
        Task<List<FlightInfo>> GetDeparturesAsync(string iataCode, int offsetMinutes, int durationMinutes);

        /// <summary>
        /// Get arrivals for a given airport within the specified time window.
        /// </summary>
        /// <param name="iataCode">The IATA airport code (e.g., "LAX").</param>
        /// <param name="offsetMinutes">Minutes offset from current time to start the window.</param>
        /// <param name="durationMinutes">Length of the time window in minutes.</param>
        /// <returns>A list of flight information records.</returns>
        Task<List<FlightInfo>> GetArrivalsAsync(string iataCode, int offsetMinutes, int durationMinutes);
    }
}