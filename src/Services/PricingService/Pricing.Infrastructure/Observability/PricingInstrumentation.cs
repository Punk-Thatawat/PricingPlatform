using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Pricing.Infrastructure.Observability;

public static class PricingInstrumentation
{
    public static readonly Counter<long> QuoteRequests = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "pricing.quote.requests",
        description: "Number of synchronous quote requests.");

    public static readonly Histogram<double> QuoteDurationMs = global::Metrics.PlatformMetrics.Meter.CreateHistogram<double>(
        "pricing.quote.duration.ms",
        unit: "ms",
        description: "Duration of synchronous quote calculation requests.");

    public static readonly Counter<long> BulkJobsQueued = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "pricing.bulk.jobs.queued",
        description: "Number of bulk quote jobs queued.");

    public static readonly Counter<long> BulkJobsCompleted = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "pricing.bulk.jobs.completed",
        description: "Number of bulk quote jobs completed.");

    public static readonly Counter<long> BulkJobsFailed = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "pricing.bulk.jobs.failed",
        description: "Number of bulk quote jobs failed.");

    public static readonly Counter<long> BulkRowsProcessed = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "pricing.bulk.rows.processed",
        description: "Number of bulk quote rows processed successfully.");

    public static readonly Counter<long> BulkRowsFailed = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "pricing.bulk.rows.failed",
        description: "Number of bulk quote rows failed.");

    public static readonly Counter<long> RuleFetchRequests = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "pricing.rules.fetch.requests",
        description: "Number of requests from PricingService to RuleService.");

    public static readonly Counter<long> PipelineCacheHits = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "pricing.pipeline.cache.hits",
        description: "Number of compiled pricing pipeline cache hits.");

    public static readonly Counter<long> PipelineCacheMisses = global::Metrics.PlatformMetrics.Meter.CreateCounter<long>(
        "pricing.pipeline.cache.misses",
        description: "Number of compiled pricing pipeline cache misses.");

    public static readonly Histogram<double> PipelineCompileDurationMs = global::Metrics.PlatformMetrics.Meter.CreateHistogram<double>(
        "pricing.pipeline.compile.duration.ms",
        unit: "ms",
        description: "Duration of compiled pricing pipeline creation.");

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return global::Tracing.PlatformActivitySource.Instance.StartActivity(name, kind);
    }
}
