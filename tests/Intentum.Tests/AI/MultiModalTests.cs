using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.AI.MultiModal;

namespace Intentum.Tests.AI;

public sealed class MultiModalTests
{
    [Fact]
    public void Fusion_WithBehaviorOnly_ReturnsBehaviorEmbedding()
    {
        var fusion = new MultiModalFusion();
        var behavior = new float[] { 0.1f, 0.2f, 0.3f };

        var result = fusion.Fuse(behavior, []);

        Assert.Equal(3, result.Length);
        Assert.Equal(0.1f, result[0]);
        Assert.Equal(0.2f, result[1]);
        Assert.Equal(0.3f, result[2]);
    }

    [Fact]
    public void Fusion_WithImageModality_ConcatenatesEmbeddings()
    {
        var fusion = new MultiModalFusion();
        var behavior = new float[] { 0.1f, 0.2f };
        var additional = new[]
        {
            new MultiModalInput(InputModality.Image, "img.jpg", new float[] { 0.3f, 0.4f })
        };

        var result = fusion.Fuse(behavior, additional);

        Assert.Equal(4, result.Length);
        Assert.Equal(0.1f, result[0]);
        Assert.Equal(0.2f, result[1]);
        Assert.Equal(0.3f, result[2]);
        Assert.Equal(0.4f, result[3]);
    }

    [Fact]
    public void Fusion_WithMultipleModalities_AveragesByModality()
    {
        var fusion = new MultiModalFusion();
        var behavior = new float[] { 1.0f, 2.0f };
        var additional = new[]
        {
            new MultiModalInput(InputModality.Text, "text1", new float[] { 3.0f, 4.0f }),
            new MultiModalInput(InputModality.Text, "text2", new float[] { 5.0f, 6.0f })
        };

        var result = fusion.Fuse(behavior, additional);

        Assert.Equal(4, result.Length);
        Assert.Equal(1.0f, result[0]);
        Assert.Equal(2.0f, result[1]);
        Assert.Equal(4.0f, result[2]);
        Assert.Equal(5.0f, result[3]);
    }

    [Fact]
    public void Fusion_WithNoBehaviorEmbedding_Throws()
    {
        var fusion = new MultiModalFusion();
        Assert.Throws<ArgumentNullException>(() => fusion.Fuse(null!, []));
    }

    [Fact]
    public void MultiModalIntentModel_Infer_OnlyBehavior_ReturnsResult()
    {
        var model = new MultiModalIntentModel();
        var space = new BehaviorSpace().Observe("user", "test");

        var result = model.Infer(space);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Name);
    }
}
