using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Pricing.Infrastructure.Integrations;

namespace Pricing.API.HealthChecks;

public sealed class RuleServiceHealthCheck : IHealthCheck
{
    private const string PingPath = "/ping";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RuleServiceOptions _options;

    public RuleServiceHealthCheck(
        IHttpClientFactory httpClientFactory,
        IOptions<RuleServiceOptions> options)
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
            return HealthCheckResult.Unhealthy("RuleService:BaseUrl is not configured.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(RuleServiceHealthCheck));
            var response = await client.GetAsync(PingPath, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"RuleService ping failed with status code {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RuleService ping failed.", ex);
        }
    }
}
