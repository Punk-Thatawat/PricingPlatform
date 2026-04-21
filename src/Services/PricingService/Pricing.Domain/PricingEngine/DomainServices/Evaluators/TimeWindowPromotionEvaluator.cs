using Pricing.Domain.PricingEngine.Rules;
using Pricing.Domain.PricingEngine.ValueObjects;

namespace Pricing.Domain.PricingEngine.DomainServices.Evaluators
{
    public static class TimeWindowPromotionEvaluator
    {
        public static PriceEffect Execute(in PriceContext ctx, in Rule rule)
        {
            if (rule.TimeWindow is null)
                throw new InvalidOperationException("TimeWindow config is missing");

            ref readonly var c = ref rule.TimeWindow;

            if (c.StartHour < 0 || c.StartHour > 23 ||
                c.EndHour < 0 || c.EndHour > 23)
                throw new InvalidOperationException("Invalid hour range");

            if (c.ApplicableDays is { Count: > 0 } &&
                !c.ApplicableDays.Contains(ctx.DayOfWeek))
                return PriceEffect.None;

            var inWindow = c.StartHour <= c.EndHour
                ? ctx.HourOfDay >= c.StartHour && ctx.HourOfDay <= c.EndHour
                : ctx.HourOfDay >= c.StartHour || ctx.HourOfDay <= c.EndHour;

            if (!inWindow)
                return PriceEffect.None;

            return c.DiscountType switch
            {
                DiscountType.FlatFee =>
                    new PriceEffect(-c.Value, 1, null),

                DiscountType.Percent =>
                    new PriceEffect(0, ClampPercent(c.Value), null),

                _ => throw new NotSupportedException()
            };
        }

        private static decimal ClampPercent(decimal percent)
        {
            if (percent <= 0)
                return 1;

            if (percent >= 100)
                return 0.0001m;

            return 1 - (percent / 100m);
        }
    }
}
