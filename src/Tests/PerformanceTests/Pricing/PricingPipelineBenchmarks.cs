using BenchmarkDotNet.Attributes;
using Pricing.Domain.PricingEngine.DomainServices;
using Pricing.Domain.PricingEngine.Rules;
using Pricing.Domain.PricingEngine.ValueObjects;

namespace PricingPlatform.PerformanceTests.Pricing;

[MemoryDiagnoser]
public class PricingPipelineBenchmarks
{
    private Rule[] _rules = Array.Empty<Rule>();
    private PricingPipelineCompiler _compiler = null!;
    private CompiledPricingPipeline _compiledPipeline = null!;
    private PriceContext _context;

    [GlobalSetup]
    public void Setup()
    {
        _rules =
        [
            new Rule
            {
                Type = RuleType.WeightTier,
                WeightTier = new WeightTierRuleConfig
                {
                    Tiers =
                    [
                        new WeightTierConfig
                        {
                            MinWeight = 0,
                            MaxWeight = 5,
                            PricingType = WeightTierPricingType.FlatFee,
                            Value = 10m
                        },
                        new WeightTierConfig
                        {
                            MinWeight = 5.01m,
                            MaxWeight = 20,
                            PricingType = WeightTierPricingType.PerKg,
                            Value = 2.5m
                        }
                    ]
                }
            },
            new Rule
            {
                Type = RuleType.RemoteAreaSurcharge,
                RemoteArea = new RemoteAreaConfig
                {
                    Zones = ["REMOTE", "ISLAND"],
                    SurchargeType = SurchargeType.Percent,
                    Value = 12m
                }
            },
            new Rule
            {
                Type = RuleType.TimeWindowPromotion,
                TimeWindow = new TimeWindowConfig
                {
                    StartHour = 18,
                    EndHour = 23,
                    ApplicableDays = [DayOfWeek.Friday, DayOfWeek.Saturday],
                    DiscountType = DiscountType.Percent,
                    Value = 5m
                }
            }
        ];

        _compiler = new PricingPipelineCompiler();
        _compiledPipeline = _compiler.Compile(_rules);
        _context = new PriceContext
        {
            BasePrice = 100m,
            Weight = 7m,
            Zone = "REMOTE",
            HourOfDay = 20,
            DayOfWeek = DayOfWeek.Friday
        };
    }

    [Benchmark]
    public CompiledPricingPipeline CompilePipeline()
    {
        return _compiler.Compile(_rules);
    }

    [Benchmark]
    public decimal ExecutePipeline()
    {
        return _compiledPipeline.Execute(_context);
    }
}
