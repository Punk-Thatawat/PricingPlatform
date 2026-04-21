namespace Pricing.Domain.PricingEngine.ValueObjects
{
    public readonly record struct PriceEffect
    {
        public decimal Additive { get; init; }
        public decimal Multiplicative { get; init; }
        public decimal? Override { get; init; }

        public static PriceEffect None => new(0, 1, null);

        public PriceEffect(decimal additive, decimal multiplicative, decimal? @override)
        {
            Additive = additive;
            Multiplicative = multiplicative <= 0 ? 1 : multiplicative;
            Override = @override;
        }
    }
}
