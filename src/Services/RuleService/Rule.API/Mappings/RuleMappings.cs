using Rule.Application.DTOs;
using Rule.Domain.Entities;

namespace Rule.API.Mappings;

public static class RuleMappings
{
    public static RuleDto MapToDto(this RuleDefinition rule)
    {
        return new RuleDto
        {
            Id = rule.Id,
            Type = rule.Type.ToString(),
            IsActive = rule.IsActive,
            Priority = rule.Priority,
            Version = rule.Version,
            ConfigJson = rule.ConfigJson,
            EffectiveFrom = rule.EffectiveFrom,
            EffectiveTo = rule.EffectiveTo
        };
    }
}
