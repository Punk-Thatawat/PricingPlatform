namespace Pricing.Domain.PricingEngine.Rules
{
    public enum SurchargeType
    {
        FlatFee,
        Percent
    }

    public class RemoteAreaConfig
    {
        public List<string> Zones { get; init; } = new();
        public SurchargeType SurchargeType { get; init; }
        public decimal Value { get; init; }
    }
}
