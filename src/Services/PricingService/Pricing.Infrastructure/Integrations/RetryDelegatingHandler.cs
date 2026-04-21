using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Pricing.Infrastructure.Integrations;

public sealed class RetryDelegatingHandler : DelegatingHandler
{
    private readonly HttpRetryOptions _options;
    private readonly ILogger<RetryDelegatingHandler> _logger;

    public RetryDelegatingHandler(
        IOptions<HttpRetryOptions> options,
        ILogger<RetryDelegatingHandler> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var maxAttempts = Math.Max(1, _options.MaxRetries + 1);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var clonedRequest = await CloneHttpRequestMessageAsync(request, cancellationToken);

            try
            {
                var response = await base.SendAsync(clonedRequest, cancellationToken);
                if (!ShouldRetry(response.StatusCode) || attempt == maxAttempts)
                {
                    return response;
                }

                response.Dispose();
                await DelayAsync(attempt, request.RequestUri, cancellationToken);
            }
            catch (HttpRequestException ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "Outbound HTTP retry {Attempt}/{MaxAttempts} for {Method} {Uri}.",
                    attempt,
                    maxAttempts,
                    request.Method,
                    request.RequestUri);

                await DelayAsync(attempt, request.RequestUri, cancellationToken);
            }
        }

        throw new InvalidOperationException("Retry handler exhausted without returning a response.");
    }

    private async Task DelayAsync(int attempt, Uri? uri, CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromMilliseconds(Math.Max(50, _options.BaseDelayMilliseconds) * attempt);

        _logger.LogWarning(
            "Transient outbound HTTP failure. Retrying attempt {Attempt} after {DelayMs}ms for {Uri}.",
            attempt + 1,
            delay.TotalMilliseconds,
            uri);

        await Task.Delay(delay, cancellationToken);
    }

    private static bool ShouldRetry(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.RequestTimeout
            || statusCode == HttpStatusCode.TooManyRequests
            || (int)statusCode >= 500;
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            var content = new ByteArrayContent(bytes);

            foreach (var header in request.Content.Headers)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Content = content;
        }

        return clone;
    }
}
