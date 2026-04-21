using System;

namespace Pricing.Domain.PricingEngine.ValueObjects
{
    public readonly struct PriceContext
    {
        public decimal BasePrice { get; init; }
        public decimal Weight { get; init; }
        public string Zone { get; init; }
        public int HourOfDay { get; init; }
        public DayOfWeek DayOfWeek { get; init; }
    }
}
