using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Rule.Infrastructure.Integrations;

namespace Rule.API.HealthChecks;

public sealed class PricingWebhookHealthCheck : IHealthCheck
{
    private const string PingPath = "/ping";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PricingWebhookOptions _options;

    public PricingWebhookHealthCheck(
        IHttpClientFactory httpClientFactory,
        IOptions<PricingWebhookOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            return HealthCheckResult.Healthy("PricingWebhook:BaseUrl is not configured; webhook delivery is disabled.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(PricingWebhookHealthCheck));
            var response = await client.GetAsync(PingPath, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Pricing webhook ping failed with status code {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Pricing webhook ping failed.", ex);
        }
    }
}
