using Rule.Domain.Enums;

namespace Rule.Domain.Entities;

public sealed class RuleDefinition
{
    public Guid Id { get; set; }
    public RuleType Type { get; set; }
    public bool IsActive { get; set; }
    public string ConfigJson { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int Version { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
