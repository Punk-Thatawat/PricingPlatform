using System;
using System.Collections.Generic;
using System.Text;

namespace Rule.Application.Interfaces
{
    public interface IPricingServiceWebhook
    {
        Task<bool> NotifyRuleChangedAsync(Guid ruleId, string action, CancellationToken ct);
    }
}
