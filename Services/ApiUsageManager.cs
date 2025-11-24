using FlightInfoApi.Configuration;
using FlightInfoApi.Models.Usage;
using Microsoft.Extensions.DependencyInjection;

namespace FlightInfoApi.Services
{
    // Manages API usage tracking and throttling logic
    public class ApiUsageManager
    {
        private readonly ApiThrottleOptions _options;
        private readonly ILogger<ApiUsageManager> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly UsageInfo _usageInfo;

        public ApiUsageManager(ApiThrottleOptions options, IServiceScopeFactory scopeFactory, ILogger<ApiUsageManager> logger)
        {
            _options = options;
            _scopeFactory = scopeFactory;
            _logger = logger;

            // Load usage info once at startup
            using var scope = _scopeFactory.CreateScope();
            var persistence = scope.ServiceProvider.GetRequiredService<UsagePersistence>();
            _usageInfo = persistence.Load();
        }

        /// <summary>
        /// Register an AeroDataBox API call and reset usage if a new cycle has started.
        /// </summary>
        public void RegisterCall()
        {
            _logger.LogInformation("New subscription cycle started at {CycleStart}", GetCurrentCycleStart());

            var now = DateTime.UtcNow;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var persistence = scope.ServiceProvider.GetRequiredService<UsagePersistence>();

                // Check if we’ve crossed into a new subscription cycle
                if (now > GetNextCycleStart())
                {
                    // Log the completed cycle to DB
                    persistence.LogCycle(_usageInfo, GetCurrentCycleStart(), GetNextCycleStart()).GetAwaiter().GetResult();

                    // Reset usage for new cycle
                    _usageInfo.CallsThisCycle = 0;
                    _usageInfo.CycleStart = _usageInfo.CycleEnd.AddDays(1);
                    _usageInfo.CycleEnd = _usageInfo.CycleStart.AddMonths(1).AddDays(-1);
                }

                _usageInfo.CallsThisCycle++;
                _logger.LogDebug("API call registered. Total calls this cycle: {Count}", _usageInfo.CallsThisCycle);

                persistence.Save(_usageInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register API call usage");
            }
        }

        /// <summary>
        /// Decide the current interval based on how many calls have been made this month.
        /// </summary>
        public TimeSpan GetCurrentInterval()
        {
            // Calculate credits remaining in the current subscription cycle
            var creditsRemaining = _options.Tier1Threshold - _usageInfo.CallsThisCycle;

            // Tier 1: High credits
            if (creditsRemaining > _options.Tier2Threshold)
            {
                _logger.LogDebug("Tier 1 interval applied");
                return TimeSpan.FromSeconds(_options.Tier1IntervalSeconds);
            }

            // Tier 2: Medium credits
            if (creditsRemaining > _options.Tier3Threshold)
            {
                _logger.LogDebug("Tier 2 interval applied");
                return TimeSpan.FromSeconds(_options.Tier2IntervalSeconds);
            }

            // Tier 3: Low credits
            _logger.LogDebug("Tier 3 interval applied");
            return TimeSpan.FromSeconds(_options.Tier3IntervalSeconds);
        }

        // Calculate the start of the current subscription cycle.
        private DateTime GetCurrentCycleStart()
        {
            var start = _options.SubscriptionStartDate;
            var now = DateTime.UtcNow;

            var monthsSinceStart = ((now.Year - start.Year) * 12) + (now.Month - start.Month);
            return start.AddMonths(monthsSinceStart);
        }

        // Calculate the start of the next subscription cycle.
        private DateTime GetNextCycleStart()
        {
            return GetCurrentCycleStart().AddMonths(1);
        }
    }
}