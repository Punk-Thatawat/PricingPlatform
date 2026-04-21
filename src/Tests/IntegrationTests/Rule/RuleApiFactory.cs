using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rule.Application.Interfaces;

namespace PricingPlatform.IntegrationTests.Rule;

public sealed class RuleApiFactory : WebApplicationFactory<global::Rule.API.Controllers.PingController>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPricingServiceWebhook>();
            services.AddSingleton<IPricingServiceWebhook>(new FakePricingServiceWebhook());
        });
    }

    private sealed class FakePricingServiceWebhook : IPricingServiceWebhook
    {
        public Task<bool> NotifyRuleChangedAsync(Guid ruleId, string action, CancellationToken ct)
        {
            return Task.FromResult(true);
        }
    }
}
