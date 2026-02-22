using Intentum.AI.Embeddings;

namespace Intentum.Tests;

public sealed class EmbeddingScoreTests
{
    [Fact]
    public void Normalize_EmptyList_ReturnsZero()
    {
        var result = EmbeddingScore.Normalize([]);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Normalize_SingleValue_ReturnsAbsoluteValue()
    {
        Assert.Equal(0.5, EmbeddingScore.Normalize([0.5]));
        Assert.Equal(1.0, EmbeddingScore.Normalize([1.5]));
        Assert.Equal(0.1, EmbeddingScore.Normalize([-0.1]));
    }

    [Fact]
    public void Normalize_MultipleValues_ReturnsL2MagnitudeClamped()
    {
        // sqrt((0.04 + 0.09 + 0.01) / 3) = sqrt(0.14 / 3) ≈ 0.216
        var result = EmbeddingScore.Normalize([0.2, -0.3, 0.1]);
        Assert.InRange(result, 0.21, 0.22);
    }

    [Fact]
    public void CosineSimilarity_IdenticalVectors_ReturnsOne()
    {
        var v = new[] { 1.0, 2.0, 3.0 };
        var result = EmbeddingScore.CosineSimilarity(v, v);
        Assert.InRange(result, 0.99, 1.01);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ReturnsHalf()
    {
        var a = new[] { 1.0, 0.0 };
        var b = new[] { 0.0, 1.0 };
        var result = EmbeddingScore.CosineSimilarity(a, b);
        Assert.InRange(result, 0.49, 0.51);
    }

    [Fact]
    public void CosineSimilarity_OppositeVectors_ReturnsZero()
    {
        var a = new[] { 1.0, 0.0 };
        var b = new[] { -1.0, 0.0 };
        var result = EmbeddingScore.CosineSimilarity(a, b);
        Assert.InRange(result, -0.01, 0.01);
    }

    [Fact]
    public void CosineSimilarity_EmptyVectors_ReturnsZero()
    {
        Assert.Equal(0.0, EmbeddingScore.CosineSimilarity([], []));
        Assert.Equal(0.0, EmbeddingScore.CosineSimilarity([1.0], []));
    }
}
