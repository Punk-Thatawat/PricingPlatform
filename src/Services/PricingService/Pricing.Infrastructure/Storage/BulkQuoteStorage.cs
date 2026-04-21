using Microsoft.Extensions.Options;
using Pricing.Application.Interfaces;
using Pricing.Infrastructure.Integrations;

namespace Pricing.Infrastructure.Storage;

public sealed class BulkQuoteStorage : IBulkQuoteStorage
{
    private readonly BulkQuoteStorageOptions _options;

    public BulkQuoteStorage(IOptions<BulkQuoteStorageOptions> options)
    {
        _options = options.Value;
    }

    public string GetInputFilePath(Guid jobId, string originalFileName)
    {
        var sanitizedFileName = Path.GetFileName(originalFileName);
        var extension = Path.GetExtension(sanitizedFileName);
        extension = string.IsNullOrWhiteSpace(extension) ? ".csv" : extension;

        return Path.Combine(GetUploadDirectory(), $"{jobId}{extension}");
    }

    public string GetResultFilePath(Guid jobId)
    {
        return Path.Combine(GetResultDirectory(), $"{jobId}.csv");
    }

    public void EnsureStorageDirectories()
    {
        Directory.CreateDirectory(GetUploadDirectory());
        Directory.CreateDirectory(GetResultDirectory());
    }

    private string GetRootDirectory()
    {
        if (!string.IsNullOrWhiteSpace(_options.RootPath))
        {
            return _options.RootPath;
        }

        return Path.Combine(AppContext.BaseDirectory, "bulk-quotes");
    }

    private string GetUploadDirectory() => Path.Combine(GetRootDirectory(), _options.UploadDirectoryName);

    private string GetResultDirectory() => Path.Combine(GetRootDirectory(), _options.ResultDirectoryName);
}
