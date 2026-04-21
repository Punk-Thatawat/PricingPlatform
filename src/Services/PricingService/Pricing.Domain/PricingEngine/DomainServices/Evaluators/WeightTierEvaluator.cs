using Pricing.Domain.PricingEngine.Rules;
using Pricing.Domain.PricingEngine.ValueObjects;

namespace Pricing.Domain.PricingEngine.DomainServices.Evaluators
{
    public static class WeightTierEvaluator
    {
        public static PriceEffect Execute(in PriceContext ctx, in Rule rule)
        {
            if (rule.WeightTier is null)
                throw new InvalidOperationException("WeightTier config is missing");

            if (ctx.Weight < 0)
                throw new ArgumentOutOfRangeException(nameof(ctx.Weight));

            ref readonly var c = ref rule.WeightTier;

            if (c.Tiers is not { Count: > 0 })
                return PriceEffect.None;

            for (int i = 0; i < c.Tiers.Count; i++)
            {
                var tier = c.Tiers[i];

                ValidateTier(tier);

                if (ctx.Weight >= tier.MinWeight &&
                    (tier.MaxWeight is null || ctx.Weight <= tier.MaxWeight.Value))
                {
                    return tier.PricingType switch
                    {
                        WeightTierPricingType.FlatFee =>
                            new PriceEffect(tier.Value, 1, null),

                        WeightTierPricingType.PerKg =>
                            new PriceEffect(ctx.Weight * tier.Value, 1, null),

                        _ => throw new NotSupportedException($"Unsupported PricingType: {tier.PricingType}")
                    };
                }
            }

            return PriceEffect.None;
        }

        private static void ValidateTier(WeightTierConfig tier)
        {
            if (tier.MinWeight < 0)
                throw new InvalidOperationException("MinWeight cannot be negative");

            if (tier.MaxWeight.HasValue &&
                tier.MaxWeight.Value < tier.MinWeight)
                throw new InvalidOperationException("MaxWeight must be >= MinWeight");

            if (tier.Value < 0)
                throw new InvalidOperationException("Tier value cannot be negative");
        }
    }
}
