namespace Pricing.Application.Interfaces;

public interface IBulkQuoteJobQueue
{
    ValueTask QueueAsync(Guid jobId, CancellationToken cancellationToken = default);
    ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken = default);
}
