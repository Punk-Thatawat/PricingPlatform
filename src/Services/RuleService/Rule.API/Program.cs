using Microsoft.OpenApi;
using Extensions.Extensions;
using FluentValidation;
using Rule.API.Configurations;
using Rule.API.HealthChecks;
using Rule.Application.Interfaces;
using Rule.Application.Validators;
using Rule.Domain.Repositories;
using Rule.Infrastructure.Integrations;
using Rule.Infrastructure.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IRuleRepository, InMemoryRuleRepository>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateRuleRequestDtoValidator>();
builder.Services.Configure<PricingWebhookOptions>(
    builder.Configuration.GetSection(PricingWebhookOptions.SectionName));
builder.Services.Configure<RateLimitingOptions>(builder.Configuration.GetSection(RateLimitingOptions.SectionName));
builder.Services.Configure<HttpRetryOptions>(builder.Configuration.GetSection(HttpRetryOptions.SectionName));
builder.Services.AddSingleton<Microsoft.Extensions.Options.IConfigureOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>, ConfigureGlobalRateLimiterOptions>();
builder.Services.AddTransient<RetryDelegatingHandler>();
builder.Services.AddRateLimiter();
builder.Services.AddHealthChecks()
    .AddCheck<PricingWebhookHealthCheck>("pricing-webhook", tags: ["ready"]);
builder.Services.AddHttpClient<IPricingServiceWebhook, PricingServiceWebhook>((serviceProvider, httpClient) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<PricingWebhookOptions>>()
        .Value;

    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        httpClient.BaseAddress = new Uri(options.BaseUrl);
    }
})
.AddHttpMessageHandler<RetryDelegatingHandler>();
builder.Services.AddHttpClient(nameof(PricingWebhookHealthCheck), (serviceProvider, httpClient) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<PricingWebhookOptions>>()
        .Value;

    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        httpClient.BaseAddress = new Uri(options.BaseUrl);
    }
})
.AddHttpMessageHandler<RetryDelegatingHandler>();
builder.AddPlatformObservability("RuleService");
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Servers.Clear();
        document.Servers.Add(new OpenApiServer
        {
            Url = "/api/rules"
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapPlatformObservability();

app.Run();
