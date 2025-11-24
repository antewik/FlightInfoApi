using System.Net;
using System.Text.Json;
using FlightInfoApi.Configuration;
using FlightInfoApi.Models;
using FlightInfoApi.Models.AeroDataBox;
using FlightInfoApi.Services.Mocks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FlightInfoApi.Services
{
    /// <summary>
    /// Service for retrieving flight information from AeroDataBox API.
    /// Provides caching, usage tracking, and optional mock data for testing.
    /// </summary>
    public class AeroDataBoxFlightInfoService : IAeroDataBoxFlightInfoService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiUsageManager _usageManager;
        private readonly MemoryCache _cache;
        private readonly MockFlightInfoService _mockFlightService;
        private readonly MockOptions _optionsMock;
        private readonly AeroDataBoxApiOptions _optionsApi;
        private readonly ILogger<AeroDataBoxFlightInfoService> _logger;

        public AeroDataBoxFlightInfoService(HttpClient httpClient,
            ApiUsageManager usageManager,
            ApiThrottleOptions optionsApiTrottle,
            UsagePersistence persistence,
            MockFlightInfoService mockFlightService,
            IOptions<MockOptions> optionsMock,
            IOptions<AeroDataBoxApiOptions> optionsApi,
            ILogger<AeroDataBoxFlightInfoService> logger)
        {
            _httpClient = httpClient;
            _usageManager = usageManager;
            _mockFlightService = mockFlightService;
            _cache = new MemoryCache(new MemoryCacheOptions());
            _optionsMock = optionsMock.Value;
            _optionsApi = optionsApi.Value;
            _logger = logger;
        }

        /// <summary>
        /// Get departures for a given airport within the specified time window.
        /// Returns mock or live data depending on configuration.
        /// </summary>
        public async Task<List<FlightInfo>> GetDeparturesAsync(string iataCode, int offsetMinutes, int durationMinutes)
        {
            if (UseMockData())
            {
                return GetMockFlights("Departure");
            }

            return await GetFlightsAsync(iataCode, "Departure",offsetMinutes, durationMinutes);
        }

        /// <summary>
        /// Get arrivals for a given airport within the specified time window.
        /// Returns mock or live data depending on configuration.
        /// </summary>
        public async Task<List<FlightInfo>> GetArrivalsAsync(string iataCode, int offsetMinutes, int durationMinutes)
        {
            if (UseMockData())
            {
                return GetMockFlights("Arrival");
            }

            return await GetFlightsAsync(iataCode, "Arrival", offsetMinutes, durationMinutes);
        }

        /// <summary>
        /// Method for retrieving flights (departures or arrivals).
        /// Uses cache if data is fresh, otherwise calls AeroDataBox API.
        /// </summary>
        private async Task<List<FlightInfo>> GetFlightsAsync(string iataCode, string direction, int offsetMinutes, int durationMinutes)
        {
            var cacheKey = $"{iataCode}:{direction}";
            var interval = _usageManager.GetCurrentInterval();

            // If cached and interval not elapsed -> return cached
            if (_cache.TryGetValue(cacheKey, out List<FlightInfo> cachedFlights) &&
                _cache.TryGetValue($"{cacheKey}:lastUpdated", out DateTime lastUpdated) &&
                (DateTime.UtcNow - lastUpdated) < interval)
            {
                _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
                return cachedFlights;
            }

            // Otherwise call AeroDataBox
            try
            {
                var apiResponse = await FetchDataAsync(iataCode, direction, offsetMinutes, durationMinutes);

                var flights = direction == "Departure"
                    ? apiResponse?.Departures?.Select(r => MapFlight(r, direction)).ToList()
                    : apiResponse?.Arrivals?.Select(r => MapFlight(r, direction)).ToList();

                flights ??= new List<FlightInfo>();

                // Track usage + update cache
                _usageManager.RegisterCall();
                _logger.LogInformation("API call registered for {IataCode} {Direction}", iataCode, direction);

                _cache.Set(cacheKey, flights);
                _cache.Set($"{cacheKey}:lastUpdated", DateTime.UtcNow);

                return flights;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching flights for {IataCode} {Direction}", iataCode, direction);
                return new List<FlightInfo>();
            }
        }

        /// <summary>
        /// Calls AeroDataBox API and parses the response.
        /// </summary>
        private async Task<AeroDataBoxApiResponse?> FetchDataAsync(
    string iataCode, string direction, int offsetMinutes, int durationMinutes)
        {
            try
            {
                var apiKey = _optionsApi.ApiKey;
                string baseUrl = _optionsApi.BaseUrl;

                var url = $"{baseUrl}{iataCode}" +
                    $"?offsetMinutes={offsetMinutes}" +
                    $"&durationMinutes={durationMinutes}" +
                    $"&withLeg=false" +
                    $"&direction={direction}" +
                    $"&withCancelled=true" +
                    $"&withCodeshared=false" +
                    $"&withCargo=false" +
                    $"&withPrivate=false" +
                    $"&withLocation=false";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("X-RapidAPI-Key", apiKey);
                request.Headers.Add("X-RapidAPI-Host", "aerodatabox.p.rapidapi.com");

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    _logger.LogInformation("No flights returned for {IataCode} {Direction}", iataCode, direction);
                    return null;
                }

                if (response.StatusCode == (HttpStatusCode)429) // Too Many Requests
                {
                    string tier = "Tier 2";
                    string? remainingRaw = response.Headers.Contains("X-RateLimit-Tier-2-Remaining")
                        ? response.Headers.GetValues("X-RateLimit-Tier-2-Remaining").FirstOrDefault()
                        : null;
                    string? resetSecondsRaw = response.Headers.Contains("X-RateLimit-Tier-2-Reset")
                        ? response.Headers.GetValues("X-RateLimit-Tier-2-Reset").FirstOrDefault()
                        : null;

                    int? remaining = int.TryParse(remainingRaw, out var remVal) ? remVal : (int?)null;
                    DateTimeOffset? resetAt = null;
                    if (int.TryParse(resetSecondsRaw, out var secondsUntilReset))
                    {
                        resetAt = DateTimeOffset.UtcNow.AddSeconds(secondsUntilReset);
                    }

                    _logger.LogWarning(
                        "Rate limit exceeded for AeroDataBox. {Tier} remaining={Remaining}, resets at {ResetAt:u}. IATA={IataCode}, Direction={Direction}",
                        tier,
                        remaining?.ToString() ?? "unknown",
                        resetAt?.ToString("u") ?? "unknown",
                        iataCode,
                        direction
                    );

                    return null;
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("Empty response body for {IataCode} {Direction}", iataCode, direction);
                    return null;
                }

                try
                {
                    var flights = JsonSerializer.Deserialize<AeroDataBoxApiResponse>(content);
                    return flights;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse AeroDataBox response: {Content}", content);
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for {IataCode} {Direction}", iataCode, direction);
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "HTTP request timed out for {IataCode} {Direction}", iataCode, direction);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching AeroDataBox data for {IataCode} {Direction}", iataCode, direction);
                return null;
            }
        }



        // Maps AeroDataBox flight record into internal FlightInfo model
        private FlightInfo MapFlight(FlightRecord record, string direction) => new FlightInfo
        {
            FlightNumber = record.Number,
            Airline = record.Airline?.Name,
            TimeScheduled = record.Movement?.ScheduledTime,
            TimeRevised = record.Movement?.RevisedTime,
            TimeRunway = record.Movement?.RunwayTime,
            Airport = record.Movement?.Airport?.Name,
            Status = record.Status,
            Gate = record.Movement?.Gate,
            Terminal = record.Movement?.Terminal,
            AircraftModel = record.Aircraft?.Model,
            Direction = direction
        };

        // Check if mock data should be used instead of real API
        private bool UseMockData()
        {
            return _optionsMock.UseMockData;
        }

        // Generate mock flights for testing/demo purposes
        private List<FlightInfo> GetMockFlights(string direction)
        {
            int mockCount = _optionsMock.MockFlightCount;
            return _mockFlightService.GenerateRandomFlights(mockCount, direction);
        }
    }
}
