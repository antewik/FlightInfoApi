using FlightInfoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightInfoApi.Controllers
{
    // Controller for flight info endpoints (acts as proxy to AeroDataBox API)
    [Route("api/[controller]")]
    [ApiController]
    public class FlightInfoController : ControllerBase
    {
        private readonly IAeroDataBoxFlightInfoService _flightInfoService;
        private readonly ILogger<FlightInfoController> _logger;

        public FlightInfoController(IAeroDataBoxFlightInfoService flightService , ILogger<FlightInfoController> logger)
        {
            _flightInfoService = flightService;
            _logger = logger;
        }

        // Returns departures for a given airport within the specified time window
        [HttpGet("departures")]
        public async Task<IActionResult> GetDepartures(string iataCode, int offsetMinutes, int durationMinutes)
        {
            try
            {            
                _logger.LogInformation("Fetching departures for {IataCode}, offset {Offset}, duration {Duration}",
                    iataCode, offsetMinutes, durationMinutes);

                var flights = await _flightInfoService.GetDeparturesAsync(iataCode, offsetMinutes, durationMinutes);

                var cutoff = DateTime.UtcNow.AddMinutes(-10);
                var limitedFlights = flights
                    .Where(f => f.TimeScheduled?.Utc != null
                                && f.TimeScheduled.Utc >= cutoff) // only keep flights not older than 10 minutes
                    .OrderBy(f => f.TimeScheduled.Utc)
                    .ToList();


                return Ok(limitedFlights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching departures for {IataCode}", iataCode);
                return StatusCode(500, "Internal server error");
            }
        }

        // Returns arrivals for a given airport within the specified time window
        [HttpGet("arrivals")]
        public async Task<IActionResult> GetArrivals(string iataCode, int offsetMinutes, int durationMinutes)
        {
            try
            {
                _logger.LogInformation("Fetching arrivals for {IataCode}, offset {Offset}, duration {Duration}",
                    iataCode, offsetMinutes, durationMinutes);

                var flights = await _flightInfoService.GetArrivalsAsync(iataCode, offsetMinutes, durationMinutes);

                var cutoff = DateTime.UtcNow.AddMinutes(-10);
                var limitedFlights = flights
                    .Where(f => f.TimeScheduled?.Utc != null
                                && f.TimeScheduled.Utc >= cutoff) // only keep flights not older than 10 minutes
                    .OrderBy(f => f.TimeScheduled.Utc)
                    .ToList();


                return Ok(limitedFlights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching arrivals for {IataCode}", iataCode);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("ping")]
        public IActionResult Ping() => Ok("FlightInfo API is awake");
    }
}