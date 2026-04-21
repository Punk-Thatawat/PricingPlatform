using Microsoft.AspNetCore.Builder;
using HealthChecks.Extensions;
using Logging.Extensions;
using Metrics.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Tracing.Extensions;

namespace Extensions.Extensions;

public static class ObservabilityWebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddPlatformObservability(this WebApplicationBuilder builder, string serviceName)
    {
        builder
            .AddPlatformLogging(serviceName)
            .Services
            .AddPlatformTracing(serviceName)
            .AddPlatformMetrics(serviceName)
            .AddPlatformHealthChecks();

        return builder;
    }
}
