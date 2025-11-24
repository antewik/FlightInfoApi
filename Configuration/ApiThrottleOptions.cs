namespace FlightInfoApi.Configuration
{
    /// <summary>
    /// Configuration options for API throttling and usage tiers.
    /// Controls how frequently AeroDataBox can be called based on subscription credit limits.
    /// Bound from appsettings.json.
    /// </summary>
    public class ApiThrottleOptions
    {
        /// <summary>
        /// Total credits available at the start of the subscription cycle.
        /// Used as the baseline for calculating remaining credits.
        /// Tier 1 applies while credits remain above the Tier 2 threshold.
        /// One call to AeroDataBox consumes one credit.
        /// </summary>
        public int Tier1Threshold { get; set; } = 1000;

        /// <summary>
        /// Credit threshold for Tier 2.
        /// Tier 2 rules apply once credits fall below this value.
        /// </summary>
        public int Tier2Threshold { get; set; } = 500;

        /// <summary>
        /// Credit threshold for Tier 3.
        /// Tier 3 rules apply once credits fall below this value.
        /// </summary>
        public int Tier3Threshold { get; set; } = 250;

        /// <summary>
        /// Interval (seconds) between calls when in Tier 1.
        /// </summary>
        public int Tier1IntervalSeconds { get; set; } = 120; // 2 min

        /// <summary>
        /// Interval (seconds) between calls when in Tier 2.
        /// </summary>
        public int Tier2IntervalSeconds { get; set; } = 240; // 4 min

        /// <summary>
        /// Interval (seconds) between calls when in Tier 3.
        /// </summary>
        public int Tier3IntervalSeconds { get; set; } = 480; // 8 min

        /// <summary>
        /// Start date of the subscription cycle.
        /// Used to calculate monthly usage resets.
        /// </summary>
        public DateTime SubscriptionStartDate { get; set; }
    }
}
