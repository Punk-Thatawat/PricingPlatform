using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Pricing.API.Configurations;

public sealed class ConfigureGlobalRateLimiterOptions : IConfigureOptions<RateLimiterOptions>
{
    private readonly IOptions<RateLimitingOptions> _rateLimitingOptions;

    public ConfigureGlobalRateLimiterOptions(IOptions<RateLimitingOptions> rateLimitingOptions)
    {
        _rateLimitingOptions = rateLimitingOptions;
    }

    public void Configure(RateLimiterOptions options)
    {
        var settings = _rateLimitingOptions.Value;

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = Math.Max(1, settings.PermitLimit),
                    Window = TimeSpan.FromSeconds(Math.Max(1, settings.WindowSeconds)),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = Math.Max(0, settings.QueueLimit)
                });
        });
    }
}
