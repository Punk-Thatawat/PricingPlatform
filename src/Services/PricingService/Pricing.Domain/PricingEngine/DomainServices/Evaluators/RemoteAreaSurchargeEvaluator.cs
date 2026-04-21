using Pricing.Domain.PricingEngine.Rules;
using Pricing.Domain.PricingEngine.ValueObjects;

namespace Pricing.Domain.PricingEngine.DomainServices.Evaluators
{
    public static class RemoteAreaSurchargeEvaluator
    {
        public static PriceEffect Execute(in PriceContext ctx, in Rule rule)
        {
            if (ctx.Zone is null)
                throw new ArgumentNullException(nameof(ctx.Zone));

            ref readonly var c = ref rule.RemoteArea;

            if (c is null)
                throw new InvalidOperationException("RemoteArea config is missing");

            if (c.SurchargeType is not SurchargeType.FlatFee and not SurchargeType.Percent)
                throw new NotSupportedException($"Unsupported surcharge type: {c.SurchargeType}");

            if (c.Zones is { Count: > 0 } && !ContainsZone(c.Zones, ctx.Zone))
                return PriceEffect.None;

            return c.SurchargeType switch
            {
                SurchargeType.FlatFee =>
                    new PriceEffect(c.Value, 1, null),

                SurchargeType.Percent =>
                    new PriceEffect(0, ClampPercent(c.Value), null),

                _ => throw new NotSupportedException()
            };
        }

        private static bool ContainsZone(List<string> zones, string zone)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i] == zone)
                    return true;
            }

            return false;
        }

        private static decimal ClampPercent(decimal percent)
        {
            if (percent <= 0)
                return 1;

            return 1 + (percent / 100m);
        }
    }
}
