using FlightInfoApi.Models;
using FlightInfoApi.Models.AeroDataBox;
using System.Text.Json;

namespace FlightInfoApi.Services.Mocks
{
    /// <summary>
    /// Service for generating mock flight data, used for testing purposes.
    /// </summary>
    public class MockFlightInfoService
    {
        private MockPools? _cachedPools;
        private readonly ILogger<MockFlightInfoService> _logger;

        public MockFlightInfoService(ILogger<MockFlightInfoService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generate a list of random flights with data.
        /// </summary>
        /// <param name="flightCount">Number of flights to generate.</param>
        /// <param name="direction">"Departure" or "Arrival".</param>
        /// <returns>List of mock flight info records.</returns>
        public List<FlightInfo> GenerateRandomFlights(int flightCount, string direction)
        {
            var now = DateTimeOffset.Now;
            var startTime = now.AddMinutes(-10);
            var random = new Random();

            var pools = GetPools();
            var flights = new List<FlightInfo>();

            _logger.LogInformation("Generating {Count} mock {Direction} flights", flightCount, direction);

            for (int i = 0; i < flightCount; i++)
            {
                var airlineIndex = random.Next(pools.Airlines.Length);
                var airline = pools.Airlines[airlineIndex];
                var airlineCode = pools.AirlineCodes[airlineIndex];
                var flightNumber = $"{airlineCode} {random.Next(100, 9999)}";

                var scheduled = startTime.AddMinutes(i * 5);
                var revised = scheduled;

                var arrivalScheduled = scheduled.AddMinutes(random.Next(60, 180));
                var arrivalRevised = arrivalScheduled;

                string status;
                if (random.Next(10) == 0)
                {
                    // 1 in 10 chance of delay
                    revised = scheduled.AddMinutes(random.Next(5, 30));
                    arrivalRevised = arrivalScheduled.AddMinutes(random.Next(5, 30));
                    status = "Delayed";
                }
                else if (scheduled < now)
                {
                    // Already past scheduled departure/arrival time
                    status = direction == "Departure" ? "Departed" : "Arrived";
                }
                else
                {
                    // Future flights → weighted random statuses
                    var weightedStatuses = new List<string>();
                    weightedStatuses.AddRange(Enumerable.Repeat("Expected", 50));
                    weightedStatuses.AddRange(Enumerable.Repeat("Unknown", 5));
                    if (direction == "Departure")
                    {
                        weightedStatuses.AddRange(Enumerable.Repeat("Boarding", 5));
                        weightedStatuses.AddRange(Enumerable.Repeat("Canceled", 3));
                        weightedStatuses.AddRange(Enumerable.Repeat("CanceledUncertain", 2));
                    }
                    else
                    {
                        weightedStatuses.AddRange(Enumerable.Repeat("Diverted", 2));
                    }
                    status = weightedStatuses[random.Next(weightedStatuses.Count)];
                }

                flights.Add(new FlightInfo
                {
                    FlightNumber = flightNumber,
                    Airline = airline,
                    TimeScheduled = new FlightTimestamp { Utc = scheduled.ToUniversalTime(), Local = scheduled },
                    TimeRevised = new FlightTimestamp { Utc = revised.ToUniversalTime(), Local = revised },
                    TimeRunway = new FlightTimestamp { Utc = revised.ToUniversalTime(), Local = revised },
                    Airport = pools.Destinations[random.Next(pools.Destinations.Length)],
                    Status = status,
                    Gate = pools.Gates[random.Next(pools.Gates.Length)],
                    Terminal = pools.Terminals[random.Next(pools.Terminals.Length)],
                    AircraftModel = pools.AircraftModels[random.Next(pools.AircraftModels.Length)],
                    Direction = direction
                });
            }

            _logger.LogInformation("Generated {Count} mock flights successfully", flights.Count);
            return flights;
        }

        /// <summary>
        /// Load mock data pools (airlines, airports, gates, etc.) from JSON file.
        /// Cached after first load for performance.
        /// </summary>
        private MockPools GetPools()
        {
            if (_cachedPools != null) return _cachedPools;

            var filePath = Path.Combine(AppContext.BaseDirectory, "App_Data", "mockPools.json");

            try
            {
                var json = File.ReadAllText(filePath);
                _cachedPools = JsonSerializer.Deserialize<MockPools>(json);
                _logger.LogInformation("Mock pools loaded from {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load mock pools from {FilePath}", filePath);
                throw;
            }

            return _cachedPools!;
        }
    }
}
