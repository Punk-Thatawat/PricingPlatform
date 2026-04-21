using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pricing.Application.Interfaces;

namespace Pricing.API.HealthChecks;

public sealed class BulkQuoteStorageHealthCheck : IHealthCheck
{
    private readonly IBulkQuoteStorage _bulkQuoteStorage;

    public BulkQuoteStorageHealthCheck(IBulkQuoteStorage bulkQuoteStorage)
    {
        _bulkQuoteStorage = bulkQuoteStorage;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _bulkQuoteStorage.EnsureStorageDirectories();
            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Bulk quote storage is not available.", ex));
        }
    }
}
