using Microsoft.AspNetCore.Mvc;
using Pricing.Application.DTOs;
using Pricing.Application.Interfaces;

namespace Pricing.API.Controllers;

[ApiController]
[Route("jobs")]
public sealed class JobsController : ControllerBase
{
    private readonly IBulkQuoteJobStore _bulkQuoteJobStore;

    public JobsController(
        IBulkQuoteJobStore bulkQuoteJobStore)
    {
        _bulkQuoteJobStore = bulkQuoteJobStore;
    }

    [HttpGet("bulk/{jobId:guid}")]
    public ActionResult<BulkQuoteJobStatusDto> GetBulkJob(Guid jobId)
    {
        var job = _bulkQuoteJobStore.Get(jobId);
        if (job is null)
        {
            return NotFound();
        }

        return Ok(MapStatus(job));
    }

    [HttpGet("bulk/{jobId:guid}/result")]
    public IActionResult DownloadBulkResult(Guid jobId)
    {
        var job = _bulkQuoteJobStore.Get(jobId);
        if (job is null)
        {
            return NotFound();
        }

        if (!string.Equals(job.Status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(MapStatus(job));
        }

        if (!System.IO.File.Exists(job.ResultFilePath))
        {
            return NotFound();
        }

        return PhysicalFile(job.ResultFilePath, "text/csv", $"{job.JobId}.csv");
    }

    private static BulkQuoteJobStatusDto MapStatus(BulkQuoteJob job)
    {
        return new BulkQuoteJobStatusDto
        {
            JobId = job.JobId,
            FileName = job.FileName,
            Status = job.Status,
            CreatedAtUtc = job.CreatedAtUtc,
            StartedAtUtc = job.StartedAtUtc,
            CompletedAtUtc = job.CompletedAtUtc,
            TotalRows = job.TotalRows,
            SucceededRows = job.SucceededRows,
            FailedRows = job.FailedRows,
            ResultFileName = string.Equals(job.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                ? $"{job.JobId}.csv"
                : null,
            ErrorMessage = job.ErrorMessage
        };
    }
}
