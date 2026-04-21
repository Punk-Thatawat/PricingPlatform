using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pricing.Application.Interfaces;
using Pricing.Domain.PricingEngine.DomainServices;
using Pricing.Domain.PricingEngine.Rules;
using Pricing.Infrastructure.Integrations;
using Pricing.Infrastructure.Observability;

namespace Pricing.Infrastructure.Caching;

public sealed class CachedPricingPipelineProvider : IPricingPipelineProvider
{
    private const string CacheKey = "pricing:compiled-pipeline";

    private readonly IMemoryCache _memoryCache;
    private readonly IRuleServiceClient _ruleServiceClient;
    private readonly PricingPipelineCompiler _compiler;
    private readonly RuleServiceOptions _options;
    private readonly ILogger<CachedPricingPipelineProvider> _logger;

    public CachedPricingPipelineProvider(
        IMemoryCache memoryCache,
        IRuleServiceClient ruleServiceClient,
        PricingPipelineCompiler compiler,
        IOptions<RuleServiceOptions> options,
        ILogger<CachedPricingPipelineProvider> logger)
    {
        _memoryCache = memoryCache;
        _ruleServiceClient = ruleServiceClient;
        _compiler = compiler;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PricingPipelineSnapshot> GetCompiledPipelineAsync(CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue<PricingPipelineSnapshot>(CacheKey, out var cachedSnapshot) && cachedSnapshot is not null)
        {
            PricingInstrumentation.PipelineCacheHits.Add(1);
            _logger.LogDebug("Compiled pricing pipeline cache hit.");
            return cachedSnapshot;
        }

        PricingInstrumentation.PipelineCacheMisses.Add(1);
        _logger.LogInformation("Compiled pricing pipeline cache miss. Rebuilding pipeline.");
        var startedAt = DateTime.UtcNow;
        using var activity = PricingInstrumentation.StartActivity("pricing.pipeline.compile");

        var activeRules = await _ruleServiceClient.GetActiveRulesAsync(cancellationToken);
        var compiledRules = activeRules
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.EffectiveFrom)
            .Select(MapRule)
            .ToArray();

        var snapshot = new PricingPipelineSnapshot
        {
            Pipeline = _compiler.Compile(compiledRules),
            Rules = activeRules,
            CompiledAtUtc = DateTime.UtcNow
        };

        _memoryCache.Set(CacheKey, snapshot, TimeSpan.FromSeconds(Math.Max(1, _options.CacheTtlSeconds)));
        PricingInstrumentation.PipelineCompileDurationMs.Record((DateTime.UtcNow - startedAt).TotalMilliseconds);
        activity?.SetTag("rules.count", activeRules.Count);

        return snapshot;
    }

    public void Invalidate()
    {
        _memoryCache.Remove(CacheKey);
    }

    private static Rule MapRule(RuleDefinitionSnapshot rule)
    {
        return rule.Type switch
        {
            nameof(RuleType.WeightTier) => new Rule
            {
                Type = RuleType.WeightTier,
                WeightTier = DeserializeConfig<WeightTierRuleConfig>(rule.ConfigJson)
            },
            nameof(RuleType.RemoteAreaSurcharge) => new Rule
            {
                Type = RuleType.RemoteAreaSurcharge,
                RemoteArea = DeserializeConfig<RemoteAreaConfig>(rule.ConfigJson)
            },
            nameof(RuleType.TimeWindowPromotion) => new Rule
            {
                Type = RuleType.TimeWindowPromotion,
                TimeWindow = DeserializeConfig<TimeWindowConfig>(rule.ConfigJson)
            },
            _ => throw new NotSupportedException($"Unsupported rule type '{rule.Type}'.")
        };
    }

    private static T DeserializeConfig<T>(string json)
    {
        var value = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return value ?? throw new InvalidOperationException($"Unable to deserialize rule config to {typeof(T).Name}.");
    }
}
