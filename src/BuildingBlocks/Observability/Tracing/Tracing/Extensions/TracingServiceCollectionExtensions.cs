using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Tracing.Extensions;

public static class TracingServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformTracing(this IServiceCollection services, string serviceName)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(PlatformActivitySource.Name)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
            });

        return services;
    }
}
