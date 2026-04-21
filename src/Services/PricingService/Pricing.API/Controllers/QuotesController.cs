using Microsoft.AspNetCore.Mvc;
using Pricing.Application.DTOs;
using Pricing.Application.Interfaces;
using Pricing.Domain.PricingEngine.ValueObjects;
using Pricing.Infrastructure.Observability;

namespace Pricing.API.Controllers;

[ApiController]
[Route("quotes")]
public sealed class QuotesController : ControllerBase
{
    private readonly IPricingPipelineProvider _pricingPipelineProvider;
    private readonly IBulkQuoteStorage _bulkQuoteStorage;
    private readonly IBulkQuoteJobStore _bulkQuoteJobStore;
    private readonly IBulkQuoteJobQueue _bulkQuoteJobQueue;

    public QuotesController(
        IPricingPipelineProvider pricingPipelineProvider,
        IBulkQuoteStorage bulkQuoteStorage,
        IBulkQuoteJobStore bulkQuoteJobStore,
        IBulkQuoteJobQueue bulkQuoteJobQueue)
    {
        _pricingPipelineProvider = pricingPipelineProvider;
        _bulkQuoteStorage = bulkQuoteStorage;
        _bulkQuoteJobStore = bulkQuoteJobStore;
        _bulkQuoteJobQueue = bulkQuoteJobQueue;
    }

    [HttpPost("price")]
    public async Task<ActionResult<PriceQuoteResponseDto>> Price(
        [FromBody] PriceQuoteRequestDto request,
        CancellationToken cancellationToken)
    {
        PricingInstrumentation.QuoteRequests.Add(1);
        var startedAt = DateTime.UtcNow;
        using var activity = PricingInstrumentation.StartActivity("pricing.quote.calculate");
        activity?.SetTag("pricing.zone", request.Zone);
        activity?.SetTag("pricing.weight", request.Weight);
        activity?.SetTag("pricing.hour_of_day", request.HourOfDay);

        var snapshot = await _pricingPipelineProvider.GetCompiledPipelineAsync(cancellationToken);

        var finalPrice = snapshot.Pipeline.Execute(new PriceContext
        {
            BasePrice = request.BasePrice,
            Weight = request.Weight,
            Zone = request.Zone,
            HourOfDay = request.HourOfDay,
            DayOfWeek = request.DayOfWeek
        });

        PricingInstrumentation.QuoteDurationMs.Record((DateTime.UtcNow - startedAt).TotalMilliseconds);
        activity?.SetTag("pricing.final_price", finalPrice);
        activity?.SetTag("pricing.rules_count", snapshot.Rules.Count);

        return Ok(new PriceQuoteResponseDto
        {
            FinalPrice = finalPrice,
            AppliedRuleCount = snapshot.Rules.Count,
            CompiledAtUtc = snapshot.CompiledAtUtc
        });
    }

    [HttpPost("bulk")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<BulkQuoteJobAcceptedDto>> Bulk(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length <= 0)
        {
            return BadRequest("CSV file is required.");
        }

        var fileName = Path.GetFileName(file.FileName);
        if (!string.Equals(Path.GetExtension(fileName), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only .csv files are supported.");
        }

        _bulkQuoteStorage.EnsureStorageDirectories();

        var jobId = Guid.NewGuid();
        using var activity = PricingInstrumentation.StartActivity("pricing.bulk.enqueue");
        activity?.SetTag("job.id", jobId);
        activity?.SetTag("file.name", fileName);
        activity?.SetTag("file.size", file.Length);
        var inputFilePath = _bulkQuoteStorage.GetInputFilePath(jobId, fileName);
        var resultFilePath = _bulkQuoteStorage.GetResultFilePath(jobId);

        await using (var stream = System.IO.File.Create(inputFilePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var job = _bulkQuoteJobStore.Create(jobId, fileName, inputFilePath, resultFilePath);
        await _bulkQuoteJobQueue.QueueAsync(job.JobId, cancellationToken);
        PricingInstrumentation.BulkJobsQueued.Add(1);

        return Accepted(new BulkQuoteJobAcceptedDto
        {
            JobId = job.JobId,
            FileName = job.FileName,
            Status = job.Status
        });
    }
}
