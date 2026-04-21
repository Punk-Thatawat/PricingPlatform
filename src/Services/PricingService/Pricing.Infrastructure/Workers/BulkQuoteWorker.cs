using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pricing.Application.Interfaces;

namespace Pricing.Infrastructure.Workers;

public sealed class BulkQuoteWorker : BackgroundService
{
    private readonly IBulkQuoteJobQueue _jobQueue;
    private readonly IBulkQuoteProcessor _processor;
    private readonly ILogger<BulkQuoteWorker> _logger;

    public BulkQuoteWorker(
        IBulkQuoteJobQueue jobQueue,
        IBulkQuoteProcessor processor,
        ILogger<BulkQuoteWorker> logger)
    {
        _jobQueue = jobQueue;
        _processor = processor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var jobId = await _jobQueue.DequeueAsync(stoppingToken);
                await _processor.ProcessAsync(jobId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk quote worker failed while processing queued job.");
            }
        }
    }
}
