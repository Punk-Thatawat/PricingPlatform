using System;
using System.Collections.Generic;
using System.Text;

namespace Rule.Application.DTOs
{
    public sealed class RuleDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = default!;
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public int Version { get; set; }
        public string ConfigJson { get; set; } = default!;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
