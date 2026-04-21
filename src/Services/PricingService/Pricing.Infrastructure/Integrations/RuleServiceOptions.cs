namespace Pricing.Infrastructure.Integrations;

public sealed class RuleServiceOptions
{
    public const string SectionName = "RuleService";

    public string BaseUrl { get; set; } = string.Empty;
    public string ActiveRulesPath { get; set; } = "/rules?isActive=true";
    public int CacheTtlSeconds { get; set; } = 300;
}
