namespace Pricing.Application.DTOs;

public sealed class PriceQuoteRequestDto
{
    public decimal BasePrice { get; init; }
    public decimal Weight { get; init; }
    public string Zone { get; init; } = string.Empty;
    public int HourOfDay { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
}
