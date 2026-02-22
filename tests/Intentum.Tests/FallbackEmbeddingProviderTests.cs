using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Resilience;
using Moq;

namespace Intentum.Tests;

/// <summary>
/// Tests for FallbackEmbeddingProvider: first succeeds, first fail second succeeds,
/// all fail, circuit breaker (cooldown).
/// </summary>
public sealed class FallbackEmbeddingProviderTests
{
    [Fact]
    public void Constructor_WithNullProviders_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new FallbackEmbeddingProvider(null!));
    }

    [Fact]
    public void Constructor_WithEmptyProviders_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new FallbackEmbeddingProvider([]));
    }

    [Fact]
    public async Task EmbedAsync_FirstProviderSucceeds_ReturnsResult()
    {
        var provider = new MockEmbeddingProvider();
        var fallback = new FallbackEmbeddingProvider([provider]);

        var result = await fallback.EmbedAsync("user:login");

        Assert.NotNull(result);
        Assert.Equal("user:login", result.Source);
    }

    [Fact]
    public void Embed_Sync_ReturnsSameAsEmbedAsync()
    {
        var provider = new MockEmbeddingProvider();
        var fallback = new FallbackEmbeddingProvider([provider]);

        var result = fallback.Embed("user:login");

        Assert.NotNull(result);
        Assert.Equal("user:login", result.Source);
    }

    [Fact]
    public async Task EmbedAsync_FirstFailsSecondSucceeds_ReturnsFromSecond()
    {
        var failingProvider = new Mock<IIntentEmbeddingProvider>();
        failingProvider.Setup(p => p.EmbedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Provider 1 failed"));

        var successProvider = new MockEmbeddingProvider();
        var fallback = new FallbackEmbeddingProvider([failingProvider.Object, successProvider]);

        var result = await fallback.EmbedAsync("user:login");

        Assert.NotNull(result);
        Assert.Equal("user:login", result.Source);
    }

    [Fact]
    public async Task EmbedAsync_AllProvidersFail_ThrowsInvalidOperationException()
    {
        var p1 = new Mock<IIntentEmbeddingProvider>();
        p1.Setup(p => p.EmbedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("P1 failed"));
        var p2 = new Mock<IIntentEmbeddingProvider>();
        p2.Setup(p => p.EmbedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("P2 failed"));

        var fallback = new FallbackEmbeddingProvider([p1.Object, p2.Object]);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fallback.EmbedAsync("user:login"));

        Assert.Equal("All embedding providers failed.", ex.Message);
        Assert.NotNull(ex.InnerException);
    }

    [Fact]
    public async Task EmbedAsync_AfterMaxFailures_SkipsProviderUntilCooldownThenRetries()
    {
        var callCount = 0;
        var failingProvider = new Mock<IIntentEmbeddingProvider>();
        failingProvider.Setup(p => p.EmbedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount <= 2)
                    throw new HttpRequestException("Fail");
                return new IntentEmbedding("user:login", 0.5);
            });

        var fallback = new FallbackEmbeddingProvider(
            [failingProvider.Object],
            maxConsecutiveFailures: 2,
            cooldownPeriod: TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAsync<InvalidOperationException>(() => fallback.EmbedAsync("user:login"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => fallback.EmbedAsync("user:login"));

        await Task.Delay(60);

        var result = await fallback.EmbedAsync("user:login");
        Assert.NotNull(result);
        Assert.Equal(3, callCount);
    }
}
