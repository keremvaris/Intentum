namespace Intentum.AI.OpenAI;

/// <summary>
/// Thrown when OpenAI API returns 429 (Too Many Requests) after exhausting retries.
/// Use <see cref="RetryAfterSeconds"/> to show the user how long to wait; use <see cref="ResponseBody"/> to show API error (e.g. insufficient_quota).
/// </summary>
public sealed class OpenAIRateLimitException : HttpRequestException
{
    /// <summary>Suggested wait time in seconds from API Retry-After header, or null if not provided.</summary>
    public double? RetryAfterSeconds { get; }

    /// <summary>Raw response body from OpenAI (often JSON with error.message / error.type, e.g. insufficient_quota).</summary>
    public string? ResponseBody { get; }

    public OpenAIRateLimitException(double? retryAfterSeconds = null, string? responseBody = null)
        : base("OpenAI rate limit (429). Wait and retry.")
    {
        RetryAfterSeconds = retryAfterSeconds;
        ResponseBody = responseBody;
    }
}
