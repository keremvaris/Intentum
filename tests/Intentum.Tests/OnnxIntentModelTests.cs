using Intentum.AI.ONNX;

namespace Intentum.Tests;

/// <summary>
/// Tests for OnnxIntentModel (constructor validation; inference tests require a valid ONNX model file).
/// </summary>
public sealed class OnnxIntentModelTests
{
    [Fact]
    public void Constructor_WhenOptionsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new OnnxIntentModel(null!));
    }

    [Fact]
    public void Constructor_WhenModelPathMissing_ThrowsArgumentException()
    {
        var options = new OnnxIntentModelOptions(
            ModelPath: Path.Combine(Path.GetTempPath(), "nonexistent_intentum_onnx_" + Guid.NewGuid().ToString("N") + ".onnx"),
            IntentLabels: ["A", "B"]);

        var ex = Assert.Throws<ArgumentException>(() => new OnnxIntentModel(options));
        Assert.Contains("not found", ex.Message);
    }
}
