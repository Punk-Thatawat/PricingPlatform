namespace Pricing.Application.DTOs;

public sealed class PriceQuoteResponseDto
{
    public decimal FinalPrice { get; init; }
    public int AppliedRuleCount { get; init; }
    public DateTime CompiledAtUtc { get; init; }
}
