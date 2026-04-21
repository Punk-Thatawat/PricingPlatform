using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Pricing.Application.Interfaces;
using Pricing.Domain.PricingEngine.DomainServices;
using Pricing.Infrastructure.Caching;
using Pricing.Infrastructure.Integrations;

namespace PricingPlatform.UnitTests.Pricing;

public sealed class CachedPricingPipelineProviderTests
{
    [Fact]
    public async Task GetCompiledPipelineAsync_CachesSnapshot_AfterFirstFetch()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var ruleClient = new CountingRuleServiceClient(new[]
        {
            new RuleDefinitionSnapshot
            {
                Id = Guid.NewGuid(),
                Type = "WeightTier",
                IsActive = true,
                Priority = 1,
                Version = 1,
                ConfigJson = "{\"tiers\":[{\"minWeight\":0,\"maxWeight\":10,\"pricingType\":0,\"value\":12.5}]}",
                EffectiveFrom = DateTime.UtcNow
            }
        });
        var provider = new CachedPricingPipelineProvider(
            cache,
            ruleClient,
            new PricingPipelineCompiler(),
            Options.Create(new RuleServiceOptions { BaseUrl = "http://rule-api", CacheTtlSeconds = 300 }),
            NullLogger<CachedPricingPipelineProvider>.Instance);

        var first = await provider.GetCompiledPipelineAsync();
        var second = await provider.GetCompiledPipelineAsync();

        Assert.Same(first, second);
        Assert.Equal(1, ruleClient.CallCount);
    }

    [Fact]
    public async Task Invalidate_RemovesCachedSnapshot_AndTriggersRefetch()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var ruleClient = new CountingRuleServiceClient(Array.Empty<RuleDefinitionSnapshot>());
        var provider = new CachedPricingPipelineProvider(
            cache,
            ruleClient,
            new PricingPipelineCompiler(),
            Options.Create(new RuleServiceOptions { BaseUrl = "http://rule-api", CacheTtlSeconds = 300 }),
            NullLogger<CachedPricingPipelineProvider>.Instance);

        await provider.GetCompiledPipelineAsync();
        provider.Invalidate();
        await provider.GetCompiledPipelineAsync();

        Assert.Equal(2, ruleClient.CallCount);
    }

    private sealed class CountingRuleServiceClient : IRuleServiceClient
    {
        private readonly IReadOnlyCollection<RuleDefinitionSnapshot> _rules;

        public CountingRuleServiceClient(IReadOnlyCollection<RuleDefinitionSnapshot> rules)
        {
            _rules = rules;
        }

        public int CallCount { get; private set; }

        public Task<IReadOnlyCollection<RuleDefinitionSnapshot>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(_rules);
        }
    }
}
