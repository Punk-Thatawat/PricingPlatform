namespace Pricing.Application.Interfaces;

public interface IBulkQuoteStorage
{
    string GetInputFilePath(Guid jobId, string originalFileName);
    string GetResultFilePath(Guid jobId);
    void EnsureStorageDirectories();
}
