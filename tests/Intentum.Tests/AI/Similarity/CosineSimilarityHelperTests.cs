using Intentum.AI.Similarity;

namespace Intentum.Tests.AI.Similarity;

public class CosineSimilarityHelperTests
{
    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
    {
        var a = new double[] { 1, 0 };
        var b = new double[] { 0, 1 };

        var result = CosineSimilarityHelper.CosineSimilarity(a, b);

        Assert.Equal(0, result, 5);
    }

    [Fact]
    public void CosineSimilarity_SameVectors_ReturnsOne()
    {
        var a = new double[] { 1, 2, 3 };

        var result = CosineSimilarityHelper.CosineSimilarity(a, a);

        Assert.Equal(1.0, result, 5);
    }

    [Fact]
    public void CosineSimilarity_OppositeVectors_ReturnsNegativeOne()
    {
        var a = new double[] { 1, 2 };
        var b = new double[] { -1, -2 };

        var result = CosineSimilarityHelper.CosineSimilarity(a, b);

        Assert.Equal(-1.0, result, 5);
    }

    [Fact]
    public void CosineSimilarityNormalized_SameVectors_ReturnsOne()
    {
        var a = new double[] { 1, 2, 3 };

        var result = CosineSimilarityHelper.CosineSimilarityNormalized(a, a);

        Assert.Equal(1.0, result, 5);
    }

    [Fact]
    public void CosineSimilarityNormalized_OppositeVectors_ReturnsZero()
    {
        var a = new double[] { 1, 2 };
        var b = new double[] { -1, -2 };

        var result = CosineSimilarityHelper.CosineSimilarityNormalized(a, b);

        Assert.Equal(0.0, result, 5);
    }

    [Fact]
    public void CosineSimilarity_DifferentLengths_ThrowsArgumentException()
    {
        var a = new double[] { 1, 2 };
        var b = new double[] { 1, 2, 3 };

        Assert.Throws<ArgumentException>(() => CosineSimilarityHelper.CosineSimilarity(a, b));
    }

    [Fact]
    public void CosineSimilarity_ZeroVector_ReturnsZero()
    {
        var a = new double[] { 0, 0 };
        var b = new double[] { 1, 2 };

        var result = CosineSimilarityHelper.CosineSimilarity(a, b);

        Assert.Equal(0, result, 5);
    }
}