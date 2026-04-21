using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Logging.Extensions;

public static class LoggingWebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddPlatformLogging(this WebApplicationBuilder builder, string serviceName)
    {
        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("service", serviceName)
                .WriteTo.Console();
        });

        return builder;
    }
}
