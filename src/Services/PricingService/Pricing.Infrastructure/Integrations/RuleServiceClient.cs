using System.Net.Http.Json;
using Pricing.Application.Interfaces;
using Microsoft.Extensions.Options;
using Pricing.Infrastructure.Observability;

namespace Pricing.Infrastructure.Integrations;

public sealed class RuleServiceClient : IRuleServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly RuleServiceOptions _options;

    public RuleServiceClient(HttpClient httpClient, IOptions<RuleServiceOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyCollection<RuleDefinitionSnapshot>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("RuleService:BaseUrl is not configured.");
        }

        PricingInstrumentation.RuleFetchRequests.Add(1);
        using var activity = PricingInstrumentation.StartActivity("pricing.rules.fetch", System.Diagnostics.ActivityKind.Client);
        activity?.SetTag("http.base_url", _options.BaseUrl);
        activity?.SetTag("http.path", _options.ActiveRulesPath);

        var response = await _httpClient.GetAsync(_options.ActiveRulesPath, cancellationToken);
        response.EnsureSuccessStatusCode();

        var rules = await response.Content.ReadFromJsonAsync<RuleDefinitionSnapshot[]>(cancellationToken: cancellationToken);
        activity?.SetTag("rules.count", rules?.Length ?? 0);

        return rules ?? Array.Empty<RuleDefinitionSnapshot>();
    }
}
