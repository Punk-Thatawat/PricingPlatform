namespace Pricing.Application.DTOs;

public sealed class BulkQuoteJobAcceptedDto
{
    public Guid JobId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
