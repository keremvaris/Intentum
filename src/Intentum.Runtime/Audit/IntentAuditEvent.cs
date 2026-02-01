using Intentum.Runtime.Policy;

namespace Intentum.Runtime.Audit;

/// <summary>
/// Append-only audit event for an intent inference or policy decision (input hash, model/policy version, user override).
/// Use for compliance: record which input, model version, policy version produced which intent/decision, and whether a human override was applied.
/// </summary>
/// <param name="InputHash">Hash of the behavior space or input (e.g. SHA256 of serialized events) for reproducibility without storing PII.</param>
/// <param name="ModelVersion">Version or identifier of the intent model used (e.g. "rule-v1", "onnx-2025-01").</param>
/// <param name="PolicyVersion">Version or identifier of the policy used (e.g. "policy-v2").</param>
/// <param name="UserOverride">True when a human override was applied (e.g. corrected intent or decision).</param>
/// <param name="RecordedAt">When the audit event was recorded (UTC).</param>
/// <param name="IntentName">Inferred intent name.</param>
/// <param name="Decision">Policy decision (Allow, Block, Observe, etc.).</param>
public sealed record IntentAuditEvent(
    string InputHash,
    string? ModelVersion,
    string? PolicyVersion,
    bool UserOverride,
    DateTimeOffset RecordedAt,
    string IntentName,
    PolicyDecision Decision);
