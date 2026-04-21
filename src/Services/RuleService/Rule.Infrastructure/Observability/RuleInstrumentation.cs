using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Rule.Infrastructure.Observability;

public static class RuleInstrumentation
{
    public static readonly Counter<long> RulesCreated = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "rule.rules.created",
        description: "Number of rules created.");

    public static readonly Counter<long> RulesUpdated = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "rule.rules.updated",
        description: "Number of rules updated.");

    public static readonly Counter<long> RulesActivated = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "rule.rules.activated",
        description: "Number of rules activated.");

    public static readonly Counter<long> RulesDeactivated = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "rule.rules.deactivated",
        description: "Number of rules deactivated.");

    public static readonly Counter<long> RulesDeleted = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "rule.rules.deleted",
        description: "Number of rules deleted.");

    public static readonly Counter<long> WebhookNotifications = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "rule.webhook.notifications",
        description: "Number of webhook notifications sent to PricingService.");

    public static readonly Counter<long> WebhookNotificationFailures = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "rule.webhook.notifications.failed",
        description: "Number of failed webhook notifications to PricingService.");

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return global::Tracing.PlatformActivitySource.Instance.StartActivity(name, kind);
    }
}
