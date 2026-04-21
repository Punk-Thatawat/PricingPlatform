using Microsoft.OpenApi;
using Extensions.Extensions;
using Pricing.API.Configurations;
using Pricing.API.HealthChecks;
using Pricing.Application.Interfaces;
using Pricing.Domain.PricingEngine.DomainServices;
using Pricing.Infrastructure.Caching;
using Pricing.Infrastructure.Integrations;
using Pricing.Infrastructure.JobStore;
using Pricing.Infrastructure.Processing;
using Pricing.Infrastructure.Queues;
using Pricing.Infrastructure.Storage;
using Pricing.Infrastructure.Workers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<PricingPipelineCompiler>();
builder.Services.AddSingleton<IPricingPipelineProvider, CachedPricingPipelineProvider>();
builder.Services.AddSingleton<IBulkQuoteStorage, BulkQuoteStorage>();
builder.Services.AddSingleton<IBulkQuoteJobStore, InMemoryBulkQuoteJobStore>();
builder.Services.AddSingleton<IBulkQuoteJobQueue, ChannelBulkQuoteJobQueue>();
builder.Services.AddSingleton<IBulkQuoteProcessor, BulkQuoteProcessor>();
builder.Services.AddHostedService<BulkQuoteWorker>();
builder.Services.Configure<RuleServiceOptions>(builder.Configuration.GetSection(RuleServiceOptions.SectionName));
builder.Services.Configure<BulkQuoteStorageOptions>(builder.Configuration.GetSection(BulkQuoteStorageOptions.SectionName));
builder.Services.Configure<RateLimitingOptions>(builder.Configuration.GetSection(RateLimitingOptions.SectionName));
builder.Services.Configure<HttpRetryOptions>(builder.Configuration.GetSection(HttpRetryOptions.SectionName));
builder.Services.AddSingleton<Microsoft.Extensions.Options.IConfigureOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>, ConfigureGlobalRateLimiterOptions>();
builder.Services.AddTransient<RetryDelegatingHandler>();
builder.Services.AddRateLimiter();
builder.Services.AddHealthChecks()
    .AddCheck<RuleServiceHealthCheck>("rule-service", tags: ["ready"])
    .AddCheck<BulkQuoteStorageHealthCheck>("bulk-storage", tags: ["ready"]);
builder.Services.AddHttpClient<IRuleServiceClient, RuleServiceClient>((serviceProvider, httpClient) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<RuleServiceOptions>>()
        .Value;

    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        httpClient.BaseAddress = new Uri(options.BaseUrl);
    }
})
.AddHttpMessageHandler<RetryDelegatingHandler>();
builder.Services.AddHttpClient(nameof(RuleServiceHealthCheck), (serviceProvider, httpClient) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<RuleServiceOptions>>()
        .Value;

    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        httpClient.BaseAddress = new Uri(options.BaseUrl);
    }
})
.AddHttpMessageHandler<RetryDelegatingHandler>();
builder.AddPlatformObservability("PricingService");
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Servers.Clear();
        document.Servers.Add(new OpenApiServer
        {
            Url = "/api/pricing"
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.Services.GetRequiredService<IBulkQuoteStorage>().EnsureStorageDirectories();
app.UseSerilogRequestLogging();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapPlatformObservability();

app.Run();
