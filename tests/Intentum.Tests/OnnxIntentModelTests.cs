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

    [Fact]
    public void Constructor_WhenIntentLabelsEmpty_ThrowsArgumentException()
    {
        using var temp = new TempFile();
        var options = new OnnxIntentModelOptions(
            ModelPath: temp.Path,
            IntentLabels: []);

        var ex = Assert.Throws<ArgumentException>(() => new OnnxIntentModel(options));
        Assert.Contains("IntentLabels must be non-empty", ex.Message);
    }

    /// <summary>Disposable temp file for tests that need an existing path (e.g. before model load).</summary>
    private sealed class TempFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.GetTempFileName();

        public void Dispose() => File.Delete(Path);
    }
}
