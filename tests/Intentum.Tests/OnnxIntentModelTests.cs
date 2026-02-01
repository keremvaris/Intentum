using System.Reflection;
using Intentum.AI.ONNX;
using Intentum.Core.Behavior;
using Microsoft.ML.OnnxRuntime;

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

    /// <summary>ConfidenceLevel is private static; test via reflection to cover branches without ONNX.</summary>
    [Theory]
    [InlineData(0.9, "High")]
    [InlineData(0.8, "High")]
    [InlineData(0.7, "Medium")]
    [InlineData(0.5, "Medium")]
    [InlineData(0.3, "Low")]
    [InlineData(0.2, "Low")]
    [InlineData(0.1, "None")]
    [InlineData(0.0, "None")]
    public void ConfidenceLevel_ReturnsExpectedLevel(double score, string expectedLevel)
    {
        var method = typeof(OnnxIntentModel).GetMethod("ConfidenceLevel",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        var result = method.Invoke(null, [score]);
        Assert.Equal(expectedLevel, result);
    }

    /// <summary>LogitsToIntent is private static; test via reflection with a real OrtValue from minimal session.</summary>
    [Fact]
    public void LogitsToIntent_WithValidOrtValue_ReturnsArgmaxAndSoftmaxConfidence()
    {
        var path = MinimalOnnxPath();
        Assert.NotNull(path);
        using var session = new InferenceSession(path);
        var inputName = session.InputNames[0];
        var outputName = session.OutputNames[0];
        var inputFloats = new[] { 0.1f, 0.2f };
        using var inputOrt = OrtValue.CreateTensorValueFromMemory(inputFloats, [1L, 2]);
        using var runOptions = new RunOptions();
        using var outputs = session.Run(runOptions, new Dictionary<string, OrtValue> { { inputName, inputOrt } }, [outputName]);
        var outputOrt = outputs[0];

        var method = typeof(OnnxIntentModel).GetMethod("LogitsToIntent", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        var result = method.Invoke(null, [outputOrt]);
        Assert.NotNull(result);
        var (intentIndex, confidence) = ((int, double))result;
        Assert.True(intentIndex >= 0);
        Assert.True(confidence >= 0 && confidence <= 1);
    }

    [Fact]
    public void Infer_WhenMinimalOnnxAvailable_ReturnsIntentFromModel()
    {
        var path = MinimalOnnxPath();
        Assert.NotNull(path);
        var options = new OnnxIntentModelOptions(
            ModelPath: path,
            IntentLabels: ["A", "B", "C"]);

        using var model = new OnnxIntentModel(options);
        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("u", "a", DateTimeOffset.UtcNow));

        var intent = model.Infer(space);

        Assert.NotNull(intent.Name);
        Assert.True(intent.Name == "A" || intent.Name == "B" || intent.Name == "C" || intent.Name == "Unknown");
        Assert.True(intent.Confidence.Score >= 0 && intent.Confidence.Score <= 1);
    }

    [Fact]
    public void Infer_WithPrecomputedVector_UsesVectorInsteadOfSpace()
    {
        var path = MinimalOnnxPath();
        Assert.NotNull(path);
        var options = new OnnxIntentModelOptions(
            ModelPath: path,
            IntentLabels: ["X", "Y", "Z"]);

        using var model = new OnnxIntentModel(options);
        var vector = new BehaviorVector(new Dictionary<string, double> { ["f0"] = 0.5, ["f1"] = 0.5 });
        var space = new BehaviorSpace();

        var intent = model.Infer(space, vector);

        Assert.NotNull(intent.Name);
        Assert.True(intent.Confidence.Score >= 0 && intent.Confidence.Score <= 1);
    }

    [Fact]
    public void Constructor_WhenIntentLabelsCountMismatchModelOutput_ThrowsArgumentException()
    {
        var path = MinimalOnnxPath();
        Assert.NotNull(path);
        var options = new OnnxIntentModelOptions(
            ModelPath: path,
            IntentLabels: ["OnlyOne"]); // model has 3 outputs

        var ex = Assert.Throws<ArgumentException>(() => new OnnxIntentModel(options));
        Assert.Contains("IntentLabels count", ex.Message);
        Assert.Contains("must match", ex.Message);
    }

    [Fact]
    public void Constructor_WhenFeatureDimensionNamesCountMismatchInput_ThrowsArgumentException()
    {
        var path = MinimalOnnxPath();
        Assert.NotNull(path);
        var options = new OnnxIntentModelOptions(
            ModelPath: path,
            IntentLabels: ["A", "B", "C"],
            FeatureDimensionNames: ["a", "b", "c"]); // model expects 2, we pass 3

        var ex = Assert.Throws<ArgumentException>(() => new OnnxIntentModel(options));
        Assert.Contains("FeatureDimensionNames", ex.Message);
        Assert.Contains("must match", ex.Message);
    }

    private static string? MinimalOnnxPath()
    {
        var dir = Path.GetDirectoryName(typeof(OnnxIntentModelTests).Assembly.Location);
        var path = Path.Combine(dir ?? ".", "fixtures", "minimal_intent.onnx");
        return File.Exists(path) ? path : null;
    }

    /// <summary>Disposable temp file for tests that need an existing path (e.g. before model load).</summary>
    private sealed class TempFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.GetTempFileName();

        public void Dispose() => File.Delete(Path);
    }
}
