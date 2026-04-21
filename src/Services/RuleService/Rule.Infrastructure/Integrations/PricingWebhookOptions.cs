namespace Rule.Infrastructure.Integrations;

public sealed class PricingWebhookOptions
{
    public const string SectionName = "PricingWebhook";

    public string BaseUrl { get; set; } = string.Empty;
    public string RuleChangedPath { get; set; } = "/webhooks/rules/changed";
}
