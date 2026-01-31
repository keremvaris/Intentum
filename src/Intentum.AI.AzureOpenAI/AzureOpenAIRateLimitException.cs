namespace Intentum.AI.AzureOpenAI;

/// <summary>
/// Thrown when Azure OpenAI API returns 429 (Too Many Requests) after exhausting retries.
/// Use <see cref="RetryAfterSeconds"/> to show the user how long to wait; use <see cref="ResponseBody"/> to show API error (e.g. quota).
/// </summary>
public sealed class AzureOpenAIRateLimitException : HttpRequestException
{
    /// <summary>Suggested wait time in seconds from API Retry-After header, or null if not provided.</summary>
    public double? RetryAfterSeconds { get; }

    /// <summary>Raw response body from Azure OpenAI (often JSON with error details).</summary>
    public string? ResponseBody { get; }

    public AzureOpenAIRateLimitException(double? retryAfterSeconds = null, string? responseBody = null)
        : base("Azure OpenAI rate limit (429). Wait and retry.")
    {
        RetryAfterSeconds = retryAfterSeconds;
        ResponseBody = responseBody;
    }
}
