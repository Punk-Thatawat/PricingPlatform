using Microsoft.AspNetCore.Builder;
using OpenTelemetry.Metrics;

namespace Metrics.Extensions;

public static class MetricsWebApplicationExtensions
{
    public static WebApplication MapPlatformMetrics(this WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint("/metrics");

        return app;
    }
}
