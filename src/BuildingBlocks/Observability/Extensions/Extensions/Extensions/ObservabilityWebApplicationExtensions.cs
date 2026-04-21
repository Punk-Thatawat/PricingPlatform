using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Metrics.Extensions;

namespace Extensions.Extensions;

public static class ObservabilityWebApplicationExtensions
{
    public static WebApplication MapPlatformObservability(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/live", new()
        {
            Predicate = registration => registration.Tags.Contains("live")
        });
        app.MapHealthChecks("/health/ready", new()
        {
            Predicate = registration => registration.Tags.Contains("ready")
        });
        app.MapPlatformMetrics();

        return app;
    }
}
