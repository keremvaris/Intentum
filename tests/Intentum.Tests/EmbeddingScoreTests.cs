using Intentum.AI.Embeddings;

namespace Intentum.Tests;

public sealed class EmbeddingScoreTests
{
    private static readonly double[] SingleHalf = [0.5];
    private static readonly double[] SingleOneAndHalf = [1.5];
    private static readonly double[] SingleMinusPointOne = [-0.1];
    private static readonly double[] MultipleValues = [0.2, -0.3, 0.1];

    [Fact]
    public void Normalize_EmptyList_ReturnsZero()
    {
        var result = EmbeddingScore.Normalize(Array.Empty<double>());
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Normalize_SingleValue_ReturnsClamped()
    {
        Assert.Equal(0.5, EmbeddingScore.Normalize(SingleHalf));
        Assert.Equal(1.0, EmbeddingScore.Normalize(SingleOneAndHalf));
        Assert.Equal(0.1, EmbeddingScore.Normalize(SingleMinusPointOne)); // uses average of absolute values
    }

    [Fact]
    public void Normalize_MultipleValues_ReturnsAverageOfAbsoluteValuesClamped()
    {
        var result = EmbeddingScore.Normalize(MultipleValues);
        Assert.InRange(result, 0.19, 0.21);
    }
}
