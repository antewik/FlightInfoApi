using FlightInfoApi.Data;
using FlightInfoApi.Models.Usage;
using System.Text.Json;

namespace FlightInfoApi.Services
{
    // Handles persistence of API usage data
    public class UsagePersistence
    {
        private readonly UsageDbContext _db;
        private readonly ILogger<UsagePersistence> _logger;

        public UsagePersistence(UsageDbContext db, ILogger<UsagePersistence> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Load usage info from JSON file, or initialize if missing
        /// </summary>
        public UsageInfo Load()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "App_Data", "usage.json");

            try
            {
                if (!File.Exists(path))
                {
                    _logger.LogInformation("Usage file not found. Initializing new usage info.");
                    return new UsageInfo
                    {
                        CallsThisCycle = 0,
                        CycleStart = DateTime.UtcNow,
                        CycleEnd = DateTime.UtcNow.AddMonths(1).AddDays(-1)
                    };
                }

                var json = File.ReadAllText(path);
                var info = JsonSerializer.Deserialize<UsageInfo>(json) ?? new UsageInfo();
                _logger.LogInformation("Usage info loaded successfully. Calls this cycle: {Count}", info.CallsThisCycle);

                return info;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load usage info.");
                return new UsageInfo();
            }
        }

        /// <summary>
        /// Save usage info to JSON file
        /// </summary>
        public void Save(UsageInfo info)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "App_Data", "usage.json");

            try
            {
                var json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);

                _logger.LogInformation("Usage info saved. Calls this cycle: {Count}", info.CallsThisCycle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save usage info");
            }
        }

        /// <summary>
        /// Log the completed cycle to the database.
        /// </summary>
        public async Task LogCycle(UsageInfo info, DateTime cycleStart, DateTime cycleEnd)
        {
            var logEntry = new ApiUsageLog
            {
                CallsThisCycle = info.CallsThisCycle,
                CycleStart = cycleStart,
                CycleEnd = cycleEnd,
                LoggedAt = DateTime.UtcNow
            };

            await DbRetryHelper.ExecuteWithRetryAsync(async () =>
            {
                _db.ApiUsageLogs.Add(logEntry);
                await _db.SaveChangesAsync();
            }, _logger);
        }
    }
}