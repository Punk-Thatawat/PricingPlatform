using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Rule.Application.Interfaces;
using Rule.Infrastructure.Observability;

namespace Rule.Infrastructure.Integrations;

public sealed class PricingServiceWebhook : IPricingServiceWebhook
{
    private readonly HttpClient _httpClient;
    private readonly PricingWebhookOptions _options;

    public PricingServiceWebhook(HttpClient httpClient, IOptions<PricingWebhookOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<bool> NotifyRuleChangedAsync(Guid ruleId, string action, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            return true;
        }

        using var activity = RuleInstrumentation.StartActivity("rule.webhook.notify", System.Diagnostics.ActivityKind.Client);
        activity?.SetTag("rule.id", ruleId);
        activity?.SetTag("rule.action", action);
        RuleInstrumentation.WebhookNotifications.Add(1);

        var response = await _httpClient.PostAsJsonAsync(
            _options.RuleChangedPath,
            new
            {
                ruleId,
                action,
                occurredAtUtc = DateTime.UtcNow
            },
            ct);

        if (!response.IsSuccessStatusCode)
        {
            RuleInstrumentation.WebhookNotificationFailures.Add(1);
            activity?.SetTag("http.status_code", (int)response.StatusCode);
        }

        return response.IsSuccessStatusCode;
    }
}
