using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Microsoft.ML.OnnxRuntime;

namespace Intentum.AI.ONNX;

/// <summary>
/// Local intent classifier using ONNX Runtime. Implements <see cref="IIntentModel"/> for offline or low-latency inference.
/// Expects model input: float tensor [1, N]; output: float tensor [1, C] (logits). Maps argmax to intent name and softmax to confidence.
/// </summary>
public sealed class OnnxIntentModel : IIntentModel, IDisposable
{
    private readonly InferenceSession _session;
    private readonly OnnxIntentModelOptions _options;
    private readonly string _inputName;
    private readonly string _outputName;
    private readonly int _inputSize;

    /// <summary>
    /// Creates an ONNX intent model from the given options. Loads the model file and validates input/output shapes.
    /// </summary>
    public OnnxIntentModel(OnnxIntentModelOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(options.ModelPath) || !File.Exists(options.ModelPath))
            throw new ArgumentException($"Model file not found: {options.ModelPath}", nameof(options));
        if (options.IntentLabels is not { Count: > 0 })
            throw new ArgumentException("IntentLabels must be non-empty.", nameof(options));

        _session = new InferenceSession(options.ModelPath);

        _inputName = options.InputName ?? _session.InputNames[0];
        _outputName = options.OutputName ?? _session.OutputNames[0];

        var inputMeta = _session.InputMetadata[_inputName];
        var outputMeta = _session.OutputMetadata[_outputName];

        _inputSize = Convert.ToInt32(inputMeta.Dimensions[^1]);
        var numClasses = Convert.ToInt32(outputMeta.Dimensions[^1]);

        if (options.IntentLabels.Count != numClasses)
            throw new ArgumentException(
                $"IntentLabels count ({options.IntentLabels.Count}) must match model output size ({numClasses}).",
                nameof(options));

        if (options.FeatureDimensionNames != null && options.FeatureDimensionNames.Count != _inputSize)
            throw new ArgumentException(
                $"FeatureDimensionNames count ({options.FeatureDimensionNames.Count}) must match model input size ({_inputSize}).",
                nameof(options));
    }

    /// <inheritdoc />
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var vector = precomputedVector ?? behaviorSpace.ToVector();
        var inputFloats = VectorToFloats(vector);

        using var inputOrt = OrtValue.CreateTensorValueFromMemory(inputFloats, [1L, _inputSize]);
        var inputs = new Dictionary<string, OrtValue> { { _inputName, inputOrt } };
        var outputNames = new[] { _outputName };

        using var outputs = _session.Run(null, inputs, outputNames);
        if (outputs.Count == 0)
            return new Intent("Unknown", [], new IntentConfidence(0, "None"), "ONNX output missing.");

        var (intentIndex, confidence) = LogitsToIntent(outputs[0]);

        var intentName = intentIndex >= 0 && intentIndex < _options.IntentLabels.Count
            ? _options.IntentLabels[intentIndex]
            : "Unknown";

        return new Intent(
            intentName,
            [],
            new IntentConfidence(confidence, ConfidenceLevel(confidence)),
            $"ONNX index {intentIndex}");
    }

    /// <inheritdoc />
    public void Dispose() => _session.Dispose();

    private float[] VectorToFloats(BehaviorVector vector)
    {
        var dims = vector.Dimensions;
        var values = new float[_inputSize];

        if (_options.FeatureDimensionNames != null)
        {
            var names = _options.FeatureDimensionNames;
            for (var i = 0; i < names.Count; i++)
                values[i] = Convert.ToSingle(dims.GetValueOrDefault(names[i], 0.0));
        }
        else
        {
            var keys = dims.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
            for (var i = 0; i < Math.Min(keys.Count, _inputSize); i++)
                values[i] = Convert.ToSingle(dims[keys[i]]);
        }

        return values;
    }

    private (int IntentIndex, double Confidence) LogitsToIntent(OrtValue outputOrt)
    {
        var span = outputOrt.GetTensorDataAsSpan<float>();
        if (span.Length == 0)
            return (-1, 0);

        var logits = new float[span.Length];
        span.CopyTo(logits);

        var maxIdx = 0;
        var maxLogit = logits[0];
        for (var i = 1; i < logits.Length; i++)
        {
            if (logits[i] > maxLogit)
            {
                maxLogit = logits[i];
                maxIdx = i;
            }
        }

        var expSum = 0.0;
        for (var i = 0; i < logits.Length; i++)
            expSum += Math.Exp(logits[i] - maxLogit);
        var confidence = expSum > 0 ? 1.0 / expSum : 0;

        return (maxIdx, Math.Clamp(confidence, 0, 1));
    }

    private static string ConfidenceLevel(double score)
    {
        if (score >= 0.8) return "High";
        if (score >= 0.5) return "Medium";
        if (score >= 0.2) return "Low";
        return "None";
    }
}
