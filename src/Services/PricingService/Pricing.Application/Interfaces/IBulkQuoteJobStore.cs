namespace Pricing.Application.Interfaces;

public interface IBulkQuoteJobStore
{
    BulkQuoteJob Create(Guid jobId, string fileName, string inputFilePath, string resultFilePath);
    BulkQuoteJob? Get(Guid jobId);
    void MarkRunning(Guid jobId);
    void MarkCompleted(Guid jobId, int totalRows, int succeededRows, int failedRows);
    void MarkFailed(Guid jobId, string errorMessage);
}
