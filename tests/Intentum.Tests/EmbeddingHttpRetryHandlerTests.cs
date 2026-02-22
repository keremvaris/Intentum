using System.Net;
using Intentum.AI.Http;

namespace Intentum.Tests;

/// <summary>
/// Tests for EmbeddingHttpRetryHandler: 200 immediate return, 429 retry then 200,
/// 429 maxAttempts onRateLimitExhausted, non-429 EnsureSuccessStatusCode.
/// </summary>
public sealed class EmbeddingHttpRetryHandlerTests
{
    [Fact]
    public async Task SendWithRetryAsync_When200_ReturnsImmediately()
    {
        var attemptCount = 0;
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("ok")
        };

        var result = await EmbeddingHttpRetryHandler.SendWithRetryAsync(
            _ =>
            {
                attemptCount++;
                return Task.FromResult(response);
            },
            (_, _) => throw new InvalidOperationException("Should not be called"));

        Assert.Equal(1, attemptCount);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task SendWithRetryAsync_When429Then200_RetriesAndReturns()
    {
        var attemptCount = 0;

        var result = await EmbeddingHttpRetryHandler.SendWithRetryAsync(
            _ =>
            {
                attemptCount++;
                if (attemptCount == 1)
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StringContent("rate limited") });
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") });
            },
            (_, _) => throw new InvalidOperationException("Should not be called"),
            maxAttempts: 3,
            maxWaitSeconds: 1);

        Assert.Equal(2, attemptCount);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task SendWithRetryAsync_When429MaxAttempts_CallsOnRateLimitExhausted()
    {
        var attemptCount = 0;
        double? capturedRetryAfter = null;
        string? capturedBody = null;

        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await EmbeddingHttpRetryHandler.SendWithRetryAsync(
                _ =>
                {
                    attemptCount++;
                    var resp = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                    {
                        Content = new StringContent("quota exceeded")
                    };
                    resp.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(60));
                    return Task.FromResult(resp);
                },
                (retryAfterSec, body) =>
                {
                    capturedRetryAfter = retryAfterSec;
                    capturedBody = body;
                    throw new HttpRequestException("Rate limit exhausted");
                },
                maxAttempts: 2,
                maxWaitSeconds: 1));

        Assert.Equal(2, attemptCount);
        Assert.Equal(60.0, capturedRetryAfter);
        Assert.Equal("quota exceeded", capturedBody);
    }

    [Fact]
    public async Task SendWithRetryAsync_WhenNon429Error_ThrowsWithoutRetry()
    {
        var attemptCount = 0;
        var badResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("server error")
        };

        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await EmbeddingHttpRetryHandler.SendWithRetryAsync(
                _ =>
                {
                    attemptCount++;
                    return Task.FromResult(badResponse);
                },
                (_, _) => throw new InvalidOperationException("Should not be called"),
                maxAttempts: 3,
                maxWaitSeconds: 1));

        Assert.Equal(1, attemptCount);
    }
}
