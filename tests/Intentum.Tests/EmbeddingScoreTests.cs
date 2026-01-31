using Intentum.AI.Embeddings;

namespace Intentum.Tests;

public sealed class EmbeddingScoreTests
{
    [Fact]
    public void Normalize_EmptyList_ReturnsZero()
    {
        var result = EmbeddingScore.Normalize(Array.Empty<double>());
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Normalize_SingleValue_ReturnsClamped()
    {
        Assert.Equal(0.5, EmbeddingScore.Normalize(new[] { 0.5 }));
        Assert.Equal(1.0, EmbeddingScore.Normalize(new[] { 1.5 }));
        Assert.Equal(0.1, EmbeddingScore.Normalize(new[] { -0.1 })); // uses average of absolute values
    }

    [Fact]
    public void Normalize_MultipleValues_ReturnsAverageOfAbsoluteValuesClamped()
    {
        var result = EmbeddingScore.Normalize(new[] { 0.2, -0.3, 0.1 });
        Assert.InRange(result, 0.19, 0.21);
    }
}
