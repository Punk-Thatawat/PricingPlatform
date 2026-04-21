namespace Pricing.Domain.PricingEngine.Rules
{
    public struct Rule
    {
        public RuleType Type { get; init; }

        public WeightTierRuleConfig WeightTier;
        public RemoteAreaConfig RemoteArea;
        public TimeWindowConfig TimeWindow;
    }
}
