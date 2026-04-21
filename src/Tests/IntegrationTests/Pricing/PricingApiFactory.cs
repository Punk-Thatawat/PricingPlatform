using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pricing.Application.Interfaces;

namespace PricingPlatform.IntegrationTests.Pricing;

public sealed class PricingApiFactory : WebApplicationFactory<global::Pricing.API.Controllers.PingController>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IRuleServiceClient>();
            services.AddSingleton<IRuleServiceClient>(new FakeRuleServiceClient(
                [
                    new RuleDefinitionSnapshot
                    {
                        Id = Guid.NewGuid(),
                        Type = "WeightTier",
                        IsActive = true,
                        Priority = 1,
                        Version = 1,
                        ConfigJson = "{\"tiers\":[{\"minWeight\":0,\"maxWeight\":10,\"pricingType\":0,\"value\":15}]}",
                        EffectiveFrom = DateTime.UtcNow
                    }
                ]));
        });
    }

    private sealed class FakeRuleServiceClient : IRuleServiceClient
    {
        private readonly IReadOnlyCollection<RuleDefinitionSnapshot> _rules;

        public FakeRuleServiceClient(IReadOnlyCollection<RuleDefinitionSnapshot> rules)
        {
            _rules = rules;
        }

        public Task<IReadOnlyCollection<RuleDefinitionSnapshot>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_rules);
        }
    }
}
