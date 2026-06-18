using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.AI.MultiModal;

namespace Intentum.Tests.AI;

public sealed class MultiModalTests
{
    private static readonly float[] BehaviorVector = [0.1f, 0.2f, 0.3f];
    private static readonly float[] ImageVector = [0.3f, 0.4f];
    private static readonly float[] TextVector1 = [3.0f, 4.0f];
    private static readonly float[] TextVector2 = [5.0f, 6.0f];

    [Fact]
    public void Fusion_WithBehaviorOnly_ReturnsBehaviorEmbedding()
    {
        var result = MultiModalFusion.Fuse(BehaviorVector, []);

        Assert.Equal(3, result.Length);
        Assert.Equal(0.1f, result[0]);
        Assert.Equal(0.2f, result[1]);
        Assert.Equal(0.3f, result[2]);
    }

    [Fact]
    public void Fusion_WithImageModality_ConcatenatesEmbeddings()
    {
        var behavior = new float[] { 0.1f, 0.2f };
        var additional = new[]
        {
            new MultiModalInput(InputModality.Image, "img.jpg", ImageVector)
        };

        var result = MultiModalFusion.Fuse(behavior, additional);

        Assert.Equal(4, result.Length);
        Assert.Equal(0.1f, result[0]);
        Assert.Equal(0.2f, result[1]);
        Assert.Equal(0.3f, result[2]);
        Assert.Equal(0.4f, result[3]);
    }

    [Fact]
    public void Fusion_WithMultipleModalities_AveragesByModality()
    {
        var behavior = new float[] { 1.0f, 2.0f };
        var additional = new[]
        {
            new MultiModalInput(InputModality.Text, "text1", TextVector1),
            new MultiModalInput(InputModality.Text, "text2", TextVector2)
        };

        var result = MultiModalFusion.Fuse(behavior, additional);

        Assert.Equal(4, result.Length);
        Assert.Equal(1.0f, result[0]);
        Assert.Equal(2.0f, result[1]);
        Assert.Equal(4.0f, result[2]);
        Assert.Equal(5.0f, result[3]);
    }

    [Fact]
    public void Fusion_WithNoBehaviorEmbedding_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => MultiModalFusion.Fuse(null!, []));
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
