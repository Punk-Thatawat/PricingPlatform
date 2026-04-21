namespace Pricing.Domain.PricingEngine.Rules
{
    public enum DiscountType
    {
        FlatFee,
        Percent
    }

    public class TimeWindowConfig
    {
        public int StartHour { get; init; }
        public int EndHour { get; init; }
        public List<DayOfWeek> ApplicableDays { get; init; } = new();
        public DiscountType DiscountType { get; init; }
        public decimal Value { get; init; }
    }
}
