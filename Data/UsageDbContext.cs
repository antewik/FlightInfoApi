using FlightInfoApi.Models.Usage;
using Microsoft.EntityFrameworkCore;

namespace FlightInfoApi.Data
{
    /// <summary>
    /// EF Core DbContext for tracking API usage logs.
    /// Provides access to the ApiUsageLogs table.
    /// </summary>
    public class UsageDbContext : DbContext
    {
        /// <summary>
        /// Initialize DbContext with configured options (connection string, provider, etc.).
        /// </summary>
        public UsageDbContext(DbContextOptions<UsageDbContext> options)
            : base(options) { }

        /// <summary>
        /// Table of API usage logs.
        /// Each entry represents one completed subscription cycle.
        /// </summary>
        public DbSet<ApiUsageLog> ApiUsageLogs { get; set; }
    }
}
