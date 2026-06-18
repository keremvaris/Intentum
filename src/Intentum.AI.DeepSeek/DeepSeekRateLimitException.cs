using System.Net;

namespace Intentum.AI.DeepSeek;

public sealed class DeepSeekRateLimitException : HttpRequestException
{
    public int? RetryAfterSeconds { get; }
    public string? ResponseBody { get; }

    public DeepSeekRateLimitException(int? retryAfterSeconds, string? responseBody)
        : base($"DeepSeek rate limit exceeded. Retry after: {retryAfterSeconds}s")
    {
        RetryAfterSeconds = retryAfterSeconds;
        ResponseBody = responseBody;
    }
}
