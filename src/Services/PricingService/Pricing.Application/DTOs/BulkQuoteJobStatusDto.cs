namespace Pricing.Application.DTOs;

public sealed class BulkQuoteJobStatusDto
{
    public Guid JobId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public int TotalRows { get; init; }
    public int SucceededRows { get; init; }
    public int FailedRows { get; init; }
    public string? ResultFileName { get; init; }
    public string? ErrorMessage { get; init; }
}
