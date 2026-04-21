using Rule.Domain.Entities;
using Rule.Domain.Enums;
using Rule.Infrastructure.Repositories;

namespace PricingPlatform.UnitTests.Rule;

public sealed class InMemoryRuleRepositoryTests
{
    [Fact]
    public async Task CreateAsync_InitializesIdentityVersionAndInactiveState()
    {
        var repository = new InMemoryRuleRepository();

        var created = await repository.CreateAsync(new RuleDefinition
        {
            Type = RuleType.WeightTier,
            IsActive = true,
            ConfigJson = "{}",
            Priority = 5,
            EffectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(1, created.Version);
        Assert.False(created.IsActive);
    }

    [Fact]
    public async Task GetAllAsync_FiltersAndOrdersRules()
    {
        var repository = new InMemoryRuleRepository();

        var first = await repository.CreateAsync(new RuleDefinition
        {
            Type = RuleType.WeightTier,
            ConfigJson = "{}",
            Priority = 10,
            EffectiveFrom = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
        });
        var second = await repository.CreateAsync(new RuleDefinition
        {
            Type = RuleType.RemoteAreaSurcharge,
            ConfigJson = "{}",
            Priority = 1,
            EffectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        await repository.SetActiveStatusAsync(first.Id, true);
        await repository.SetActiveStatusAsync(second.Id, true);

        var rules = await repository.GetAllAsync(true);

        Assert.Collection(
            rules,
            item => Assert.Equal(second.Id, item.Id),
            item => Assert.Equal(first.Id, item.Id));
    }
}
