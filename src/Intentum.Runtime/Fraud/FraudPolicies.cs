using Intentum.Runtime.Policy;

namespace Intentum.Runtime.Fraud;

/// <summary>
/// Pre-built fraud detection policies for common deployment scenarios.
/// </summary>
public static class FraudPolicies
{
    /// <summary>
    /// Standard fraud detection policy: block high-confidence fraud, escalate suspicious,
    /// require additional auth for medium risk, allow low risk.
    /// </summary>
    public static IntentPolicy Standard() => new IntentPolicyBuilder()
        .Block("HighRiskFraud", i =>
            i.Name is "AccountTakeover" or "CredentialStuffing" or "PaymentFraud" &&
            i.Confidence.Score >= 0.8)
        .Escalate("SuspiciousBehavior", i =>
            i.Name is "AccountTakeover" or "CredentialStuffing" or "PaymentFraud" &&
            i.Confidence.Score >= 0.6)
        .RequireAuth("MediumRisk", i =>
            i.Confidence.Score >= 0.4 && i.Confidence.Level is "Medium")
        .RateLimit("HighFrequency", i => i.Signals.Count > 10)
        .Allow("LegitimateRecovery", i => i.Name == "AccountRecovery" && i.Confidence.Score >= 0.7)
        .Allow("LowRisk", i => i.Confidence.Score < 0.4)
        .Observe("Default", _ => true)
        .Build();

    /// <summary>
    /// Strict policy: lower thresholds for blocking, suitable for high-value transactions.
    /// </summary>
    public static IntentPolicy Strict() => new IntentPolicyBuilder()
        .Block("AnyFraudDetected", i =>
            i.Name is "AccountTakeover" or "CredentialStuffing" or "PaymentFraud" &&
            i.Confidence.Score >= 0.5)
        .Escalate("Suspicious", i => i.Confidence.Score >= 0.3)
        .RequireAuth("AllMedium", i => i.Confidence.Level is "Medium" or "Low")
        .Allow("ConfirmedSafe", i => i.Name == "AccountRecovery" && i.Confidence.Score >= 0.85)
        .Observe("Default", _ => true)
        .Build();
}
