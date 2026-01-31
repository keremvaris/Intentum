using System.Diagnostics;

namespace Intentum.Observability;

/// <summary>
/// OpenTelemetry ActivitySource for Intentum spans (inference, policy evaluation).
/// Add this source to your TracerProvider: builder.AddSource(IntentumActivitySource.Source.Name).
/// </summary>
public static class IntentumActivitySource
{
    /// <summary>
    /// Activity source name for Intentum traces.
    /// </summary>
    public static readonly ActivitySource Source = new("Intentum", "1.0.0");

    /// <summary>
    /// Span name for intent inference.
    /// </summary>
    public const string InferSpanName = "intentum.infer";

    /// <summary>
    /// Span name for policy evaluation.
    /// </summary>
    public const string PolicyEvaluateSpanName = "intentum.policy.evaluate";
}
