namespace Pricing.Infrastructure.Integrations;

public sealed class BulkQuoteStorageOptions
{
    public const string SectionName = "BulkQuoteStorage";

    public string RootPath { get; set; } = string.Empty;
    public string UploadDirectoryName { get; set; } = "uploads";
    public string ResultDirectoryName { get; set; } = "results";
}
