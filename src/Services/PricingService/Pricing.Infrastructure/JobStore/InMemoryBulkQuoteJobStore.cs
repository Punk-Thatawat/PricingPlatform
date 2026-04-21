using System.Collections.Concurrent;
using Pricing.Application.Interfaces;

namespace Pricing.Infrastructure.JobStore;

public sealed class InMemoryBulkQuoteJobStore : IBulkQuoteJobStore
{
    private readonly ConcurrentDictionary<Guid, BulkQuoteJob> _jobs = new();

    public BulkQuoteJob Create(Guid jobId, string fileName, string inputFilePath, string resultFilePath)
    {
        var job = new BulkQuoteJob
        {
            JobId = jobId,
            FileName = fileName,
            InputFilePath = inputFilePath,
            ResultFilePath = resultFilePath,
            Status = "Queued",
            CreatedAtUtc = DateTime.UtcNow
        };

        _jobs[job.JobId] = job;
        return job;
    }

    public BulkQuoteJob? Get(Guid jobId)
    {
        _jobs.TryGetValue(jobId, out var job);
        return job;
    }

    public void MarkRunning(Guid jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.Status = "Processing";
            job.StartedAtUtc = DateTime.UtcNow;
            job.ErrorMessage = null;
        }
    }

    public void MarkCompleted(Guid jobId, int totalRows, int succeededRows, int failedRows)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.Status = "Completed";
            job.TotalRows = totalRows;
            job.SucceededRows = succeededRows;
            job.FailedRows = failedRows;
            job.CompletedAtUtc = DateTime.UtcNow;
            job.ErrorMessage = null;
        }
    }

    public void MarkFailed(Guid jobId, string errorMessage)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.Status = "Failed";
            job.ErrorMessage = errorMessage;
            job.CompletedAtUtc = DateTime.UtcNow;
        }
    }
}
