namespace Pricing.Application.Interfaces;

public interface IBulkQuoteProcessor
{
    Task ProcessAsync(Guid jobId, CancellationToken cancellationToken = default);
}
