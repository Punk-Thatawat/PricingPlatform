namespace Pricing.Application.Interfaces;

public sealed class BulkQuoteJob
{
    public Guid JobId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string InputFilePath { get; init; } = string.Empty;
    public string ResultFilePath { get; init; } = string.Empty;
    public string Status { get; set; } = "Queued";
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int TotalRows { get; set; }
    public int SucceededRows { get; set; }
    public int FailedRows { get; set; }
    public string? ErrorMessage { get; set; }
}
