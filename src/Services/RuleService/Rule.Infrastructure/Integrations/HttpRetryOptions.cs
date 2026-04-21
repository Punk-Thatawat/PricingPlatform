namespace Rule.Infrastructure.Integrations;

public sealed class HttpRetryOptions
{
    public const string SectionName = "HttpRetry";

    public int MaxRetries { get; set; } = 3;
    public int BaseDelayMilliseconds { get; set; } = 200;
}
