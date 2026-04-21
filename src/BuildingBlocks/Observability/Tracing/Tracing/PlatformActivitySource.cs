using System.Diagnostics;

namespace Tracing;

public static class PlatformActivitySource
{
    public const string Name = "PricingPlatform";

    public static readonly ActivitySource Instance = new(Name);
}
