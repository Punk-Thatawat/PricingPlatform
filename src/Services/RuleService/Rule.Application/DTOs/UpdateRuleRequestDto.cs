using System;
using System.Collections.Generic;
using System.Text;
using Rule.Domain.Enums;

namespace Rule.Application.DTOs
{
    public sealed class UpdateRuleRequestDto
    {
        public RuleType Type { get; init; }
        public bool IsActive { get; init; }
        public string ConfigJson { get; init; } = string.Empty;
        public int Priority { get; init; }
        public DateTime EffectiveFrom { get; init; }
        public DateTime? EffectiveTo { get; init; }
    }
}
