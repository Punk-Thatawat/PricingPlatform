using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Extensions;

public static class HealthChecksServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy(),
                tags: ["live", "ready"]);

        return services;
    }
}
