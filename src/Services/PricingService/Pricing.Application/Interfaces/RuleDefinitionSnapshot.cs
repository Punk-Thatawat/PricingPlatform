namespace Pricing.Application.Interfaces;

public sealed class RuleDefinitionSnapshot
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int Priority { get; init; }
    public int Version { get; init; }
    public string ConfigJson { get; init; } = string.Empty;
    public DateTime EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
}
