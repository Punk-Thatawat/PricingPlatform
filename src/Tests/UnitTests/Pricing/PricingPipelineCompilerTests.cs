using Pricing.Domain.PricingEngine.DomainServices;
using Pricing.Domain.PricingEngine.Rules;
using Pricing.Domain.PricingEngine.ValueObjects;
using PricingRule = Pricing.Domain.PricingEngine.Rules.Rule;

namespace PricingPlatform.UnitTests.Pricing;

public sealed class PricingPipelineCompilerTests
{
    [Fact]
    public void Compile_AndExecute_AppliesAdditiveAndMultiplicativeRulesInOrder()
    {
        var compiler = new PricingPipelineCompiler();
        var rules = new[]
        {
            new PricingRule
            {
                Type = RuleType.WeightTier,
                WeightTier = new WeightTierRuleConfig
                {
                    Tiers =
                    [
                        new WeightTierConfig
                        {
                            MinWeight = 0,
                            MaxWeight = 10,
                            PricingType = WeightTierPricingType.FlatFee,
                            Value = 15m
                        }
                    ]
                }
            },
            new PricingRule
            {
                Type = RuleType.RemoteAreaSurcharge,
                RemoteArea = new RemoteAreaConfig
                {
                    Zones = ["REMOTE"],
                    SurchargeType = SurchargeType.Percent,
                    Value = 10m
                }
            },
            new PricingRule
            {
                Type = RuleType.TimeWindowPromotion,
                TimeWindow = new TimeWindowConfig
                {
                    StartHour = 8,
                    EndHour = 12,
                    ApplicableDays = [DayOfWeek.Monday],
                    DiscountType = DiscountType.FlatFee,
                    Value = 5m
                }
            }
        };

        var pipeline = compiler.Compile(rules);
        var result = pipeline.Execute(new PriceContext
        {
            BasePrice = 100m,
            Weight = 5m,
            Zone = "REMOTE",
            HourOfDay = 10,
            DayOfWeek = DayOfWeek.Monday
        });

        Assert.Equal(121m, result);
    }

    [Fact]
    public void Execute_WithNoMatchingRules_ReturnsBasePrice()
    {
        var compiler = new PricingPipelineCompiler();
        var rules = new[]
        {
            new PricingRule
            {
                Type = RuleType.RemoteAreaSurcharge,
                RemoteArea = new RemoteAreaConfig
                {
                    Zones = ["REMOTE"],
                    SurchargeType = SurchargeType.FlatFee,
                    Value = 25m
                }
            }
        };

        var pipeline = compiler.Compile(rules);
        var result = pipeline.Execute(new PriceContext
        {
            BasePrice = 200m,
            Weight = 2m,
            Zone = "LOCAL",
            HourOfDay = 14,
            DayOfWeek = DayOfWeek.Wednesday
        });

        Assert.Equal(200m, result);
    }
}
