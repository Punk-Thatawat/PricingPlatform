using System.Globalization;
using Pricing.Application.Interfaces;
using Pricing.Application.DTOs;
using Pricing.Domain.PricingEngine.ValueObjects;
using Pricing.Infrastructure.Observability;

namespace Pricing.Infrastructure.Processing;

public sealed class BulkQuoteProcessor : IBulkQuoteProcessor
{
    private readonly IBulkQuoteJobStore _jobStore;
    private readonly IPricingPipelineProvider _pricingPipelineProvider;

    public BulkQuoteProcessor(
        IBulkQuoteJobStore jobStore,
        IPricingPipelineProvider pricingPipelineProvider)
    {
        _jobStore = jobStore;
        _pricingPipelineProvider = pricingPipelineProvider;
    }

    public async Task ProcessAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = _jobStore.Get(jobId) ?? throw new InvalidOperationException($"Bulk quote job '{jobId}' was not found.");
        var startedAt = DateTime.UtcNow;
        using var activity = PricingInstrumentation.StartActivity("pricing.bulk.process");
        activity?.SetTag("job.id", jobId);
        activity?.SetTag("file.name", job.FileName);

        _jobStore.MarkRunning(jobId);

        try
        {
            var snapshot = await _pricingPipelineProvider.GetCompiledPipelineAsync(cancellationToken);

            using var reader = new StreamReader(job.InputFilePath);
            using var writer = new StreamWriter(job.ResultFilePath, false);

            var headerLine = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                throw new InvalidOperationException("CSV header is missing.");
            }

            var headers = CsvLineParser.Parse(headerLine);
            var headerMap = BuildHeaderMap(headers);

            await writer.WriteLineAsync("BasePrice,Weight,Zone,HourOfDay,DayOfWeek,FinalPrice,ErrorMessage");

            var totalRows = 0;
            var succeededRows = 0;
            var failedRows = 0;

            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                totalRows++;

                try
                {
                    var columns = CsvLineParser.Parse(line);
                    var request = MapRequest(columns, headerMap);
                    var finalPrice = snapshot.Pipeline.Execute(new PriceContext
                    {
                        BasePrice = request.BasePrice,
                        Weight = request.Weight,
                        Zone = request.Zone,
                        HourOfDay = request.HourOfDay,
                        DayOfWeek = request.DayOfWeek
                    });

                    await writer.WriteLineAsync(string.Join(",",
                        CsvLineParser.Escape(request.BasePrice.ToString(CultureInfo.InvariantCulture)),
                        CsvLineParser.Escape(request.Weight.ToString(CultureInfo.InvariantCulture)),
                        CsvLineParser.Escape(request.Zone),
                        CsvLineParser.Escape(request.HourOfDay.ToString(CultureInfo.InvariantCulture)),
                        CsvLineParser.Escape(request.DayOfWeek.ToString()),
                        CsvLineParser.Escape(finalPrice.ToString(CultureInfo.InvariantCulture)),
                        string.Empty));

                    succeededRows++;
                }
                catch (Exception ex)
                {
                    var outputColumns = CsvLineParser.Parse(line);
                    while (outputColumns.Count < 5)
                    {
                        outputColumns.Add(string.Empty);
                    }

                    outputColumns.Add(string.Empty);
                    outputColumns.Add(ex.Message);

                    await writer.WriteLineAsync(string.Join(",", outputColumns.Select(CsvLineParser.Escape)));
                    failedRows++;
                }
            }

            _jobStore.MarkCompleted(jobId, totalRows, succeededRows, failedRows);
            PricingInstrumentation.BulkJobsCompleted.Add(1);
            PricingInstrumentation.BulkRowsProcessed.Add(succeededRows);
            PricingInstrumentation.BulkRowsFailed.Add(failedRows);
            activity?.SetTag("rows.total", totalRows);
            activity?.SetTag("rows.succeeded", succeededRows);
            activity?.SetTag("rows.failed", failedRows);
            activity?.SetTag("duration.ms", (DateTime.UtcNow - startedAt).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _jobStore.MarkFailed(jobId, ex.Message);
            PricingInstrumentation.BulkJobsFailed.Add(1);
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);
            throw;
        }
    }

    private static Dictionary<string, int> BuildHeaderMap(IReadOnlyList<string> headers)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < headers.Count; i++)
        {
            map[headers[i].Trim()] = i;
        }

        string[] requiredHeaders = ["BasePrice", "Weight", "Zone", "HourOfDay", "DayOfWeek"];
        foreach (var requiredHeader in requiredHeaders)
        {
            if (!map.ContainsKey(requiredHeader))
            {
                throw new InvalidOperationException($"CSV header '{requiredHeader}' is required.");
            }
        }

        return map;
    }

    private static PriceQuoteRequestDto MapRequest(IReadOnlyList<string> columns, IReadOnlyDictionary<string, int> headerMap)
    {
        return new PriceQuoteRequestDto
        {
            BasePrice = decimal.Parse(GetColumnValue(columns, headerMap, "BasePrice"), CultureInfo.InvariantCulture),
            Weight = decimal.Parse(GetColumnValue(columns, headerMap, "Weight"), CultureInfo.InvariantCulture),
            Zone = GetColumnValue(columns, headerMap, "Zone"),
            HourOfDay = int.Parse(GetColumnValue(columns, headerMap, "HourOfDay"), CultureInfo.InvariantCulture),
            DayOfWeek = Enum.Parse<DayOfWeek>(GetColumnValue(columns, headerMap, "DayOfWeek"), true)
        };
    }

    private static string GetColumnValue(IReadOnlyList<string> columns, IReadOnlyDictionary<string, int> headerMap, string headerName)
    {
        var index = headerMap[headerName];
        if (index >= columns.Count)
        {
            throw new InvalidOperationException($"CSV row is missing value for '{headerName}'.");
        }

        return columns[index].Trim();
    }
}
