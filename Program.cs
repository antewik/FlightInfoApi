using FlightInfoApi.Configuration;
using FlightInfoApi.Data;
using FlightInfoApi.Services;
using FlightInfoApi.Services.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FlightInfoApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ----------------------------
            // Configure services (DI container)
            // ----------------------------

            // Controllers (API endpoints)
            builder.Services.AddControllers();

            // API documentation (OpenAPI/Swagger)
            builder.Services.AddOpenApi();

            // HTTP client for AeroDataBox service
            builder.Services.AddHttpClient<IAeroDataBoxFlightInfoService, AeroDataBoxFlightInfoService>();


            // Persistence and usage tracking
            builder.Services.AddScoped<UsagePersistence>();
            builder.Services.AddSingleton<ApiUsageManager>();

            // Mock services - for testing
            builder.Services.AddSingleton<MockFlightInfoService>();


            builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ApiThrottleOptions>>().Value);

            // Bind configuration options from appsettings.json
            builder.Services.Configure<ApiThrottleOptions>(builder.Configuration.GetSection("ApiThrottleOptions"));
            builder.Services.Configure<AeroDataBoxApiOptions>(builder.Configuration.GetSection("AeroDataBoxApiOptions"));
            builder.Services.Configure<MockOptions>(builder.Configuration.GetSection("MockOptions"));

            // Database context (EF Core)
            builder.Services.AddDbContext<UsageDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null
                    )
                );

                // Extra logging only in development
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging()
                    .LogTo(Console.WriteLine, LogLevel.Information);
                }
            });

            var app = builder.Build();

            // ----------------------------
            // Configure middleware pipeline
            // ----------------------------

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            // Add Swagger UI
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "v1");
            });

            app.Run();
        }
    }
}
