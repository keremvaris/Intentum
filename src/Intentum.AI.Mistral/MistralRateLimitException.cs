namespace Intentum.AI.Mistral;

/// <summary>
/// Thrown when Mistral API returns 429 (Too Many Requests) after exhausting retries.
/// Use <see cref="RetryAfterSeconds"/> to show the user how long to wait; use <see cref="ResponseBody"/> to show API error (e.g. quota).
/// </summary>
public sealed class MistralRateLimitException : HttpRequestException
{
    /// <summary>Suggested wait time in seconds from API Retry-After header, or null if not provided.</summary>
    public double? RetryAfterSeconds { get; }

    /// <summary>Raw response body from Mistral (often JSON with error details).</summary>
    public string? ResponseBody { get; }

    public MistralRateLimitException(double? retryAfterSeconds = null, string? responseBody = null)
        : base("Mistral rate limit (429). Wait and retry.")
    {
        RetryAfterSeconds = retryAfterSeconds;
        ResponseBody = responseBody;
    }
}
