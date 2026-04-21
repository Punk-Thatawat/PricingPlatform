using System;
using System.Collections.Generic;
using System.Text;
using Rule.Domain.Enums;

namespace Rule.Application.DTOs
{
    public sealed class CreateRuleRequestDto
    {
        public RuleType Type { get; set; }
        public bool IsActive { get; set; }
        public string ConfigJson { get; set; } = default!;
        public int Priority { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
