using System.Threading.Channels;
using Pricing.Application.Interfaces;

namespace Pricing.Infrastructure.Queues;

public sealed class ChannelBulkQuoteJobQueue : IBulkQuoteJobQueue
{
    private readonly Channel<Guid> _queue = Channel.CreateUnbounded<Guid>();

    public ValueTask QueueAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(jobId, cancellationToken);
    }

    public ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}
