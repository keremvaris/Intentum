using System.Net;

namespace Intentum.AI.Http;

/// <summary>
/// Shared HTTP retry handler for embedding providers. Retries on 429 (Too Many Requests)
/// with exponential backoff and Retry-After header support.
/// </summary>
public static class EmbeddingHttpRetryHandler
{
    /// <summary>
    /// Sends an HTTP request with retry logic for rate limiting (429).
    /// </summary>
    /// <param name="sendRequest">Factory that produces the HTTP request for each attempt.</param>
    /// <param name="onRateLimitExhausted">Called when all retries are exhausted on 429. Should throw a provider-specific exception.</param>
    /// <param name="maxAttempts">Maximum number of retry attempts.</param>
    /// <param name="maxWaitSeconds">Maximum wait time in seconds per retry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A successful HttpResponseMessage.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "S3966:Objects should not be disposed more than once", Justification = "Dispose only in 429-exhausted path; loop start disposes previous attempt's response.")]
    public static async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> sendRequest,
        Action<double?, string?> onRateLimitExhausted,
        int maxAttempts = 5,
        int maxWaitSeconds = 90,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage? response = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            response?.Dispose();
            response = await sendRequest(cancellationToken);

            if (response.IsSuccessStatusCode)
                return response;

            if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt == maxAttempts)
            {
                var retryAfterSec = response.Headers.RetryAfter?.Delta?.TotalSeconds;
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                response.Dispose();
                onRateLimitExhausted(retryAfterSec, body);
            }

            if (response.StatusCode != HttpStatusCode.TooManyRequests)
                response.EnsureSuccessStatusCode();

            var delay = TimeSpan.FromSeconds(5 * attempt);
            if (response.Headers.RetryAfter?.Delta is { } retryAfter)
                delay = TimeSpan.FromSeconds(Math.Min(retryAfter.TotalSeconds, maxWaitSeconds));

            await Task.Delay(delay, cancellationToken);
        }

        response!.EnsureSuccessStatusCode();
        return response;
    }
}
