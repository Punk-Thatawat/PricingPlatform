using System.Diagnostics.Metrics;

namespace Metrics;

public static class PlatformMetrics
{
    public const string Name = "PricingPlatform";

    public static readonly Meter Meter = new(Name);
}
