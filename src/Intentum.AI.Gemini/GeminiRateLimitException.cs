namespace Intentum.AI.Gemini;

/// <summary>
/// Thrown when Gemini API returns 429 (Too Many Requests) after exhausting retries.
/// Use <see cref="RetryAfterSeconds"/> to show the user how long to wait; use <see cref="ResponseBody"/> to show API error (e.g. quota).
/// </summary>
public sealed class GeminiRateLimitException : HttpRequestException
{
    /// <summary>Suggested wait time in seconds from API Retry-After header, or null if not provided.</summary>
    public double? RetryAfterSeconds { get; }

    /// <summary>Raw response body from Gemini (often JSON with error details).</summary>
    public string? ResponseBody { get; }

    public GeminiRateLimitException(double? retryAfterSeconds = null, string? responseBody = null)
        : base("Gemini rate limit (429). Wait and retry.")
    {
        RetryAfterSeconds = retryAfterSeconds;
        ResponseBody = responseBody;
    }
}
