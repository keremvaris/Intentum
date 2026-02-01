namespace Intentum.AI.ONNX;

/// <summary>
/// Options for the ONNX-based local intent classifier.
/// </summary>
/// <param name="ModelPath">Path to the ONNX model file. Expected: input tensor [1, N] float, output tensor [1, C] float (logits).</param>
/// <param name="IntentLabels">Intent names in order of model output indices (index 0 = first label, etc.). Length must match model output size C.</param>
/// <param name="FeatureDimensionNames">Dimension names (e.g. "user:click", "user:login") in order of model input indices. Length must match model input size N. When null, dimensions from the behavior vector are used in sorted order (model input size must match).</param>
/// <param name="InputName">Optional. ONNX input tensor name; when null, the first input is used.</param>
/// <param name="OutputName">Optional. ONNX output tensor name; when null, the first output is used.</param>
public sealed record OnnxIntentModelOptions(
    string ModelPath,
    IReadOnlyList<string> IntentLabels,
    IReadOnlyList<string>? FeatureDimensionNames = null,
    string? InputName = null,
    string? OutputName = null);
