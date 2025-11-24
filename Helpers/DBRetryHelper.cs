using Npgsql;
using Polly;

/// <summary>
/// Helper for executing database operations with retry logic.
/// Uses Polly to handle temporary SQL and timeout errors.
/// </summary>
public static class DbRetryHelper
{
    /// <summary>
    /// Execute an async operation with retry support (no return value).
    /// </summary>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="logger">Logger used to record retry attempts.</param>
    public static async Task ExecuteWithRetryAsync(Func<Task> operation, ILogger logger)
    {
        var retryPolicy = CreateRetryPolicy(logger);
        await retryPolicy.ExecuteAsync(operation);
    }

    /// <summary>
    /// Create a retry policy for temporary SQL and timeout errors.
    /// Retries up to 3 times.
    /// </summary>
    /// <param name="logger">Logger used to record retry attempts.</param>
    /// <returns>A configured Polly async policy.</returns>
    private static IAsyncPolicy CreateRetryPolicy(ILogger logger)
    {
        return Policy
            .Handle<PostgresException>(ex =>
                ex.SqlState == "08006" || // connection failure
                ex.SqlState == "08001" || // cannot establish connection
                ex.SqlState == "57P01" || // admin shutdown
                ex.SqlState == "53300" || // too many connections
                ex.SqlState == "40001"    // serialization failure
            )
            .Or<NpgsqlException>(ex => ex.IsTransient)
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (exception, timespan, retryCount, context) =>
                {
                    logger.LogWarning(exception,
                        "Retry {RetryCount} after {Delay} due to transient DB error",
                        retryCount, timespan);
                });
    }
}