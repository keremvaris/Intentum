using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Intentum.AspNetCore.HealthChecks;

/// <summary>
/// Health check for policy engine.
/// </summary>
public sealed class PolicyEngineHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var testIntent = new Intent(
                Name: "HealthCheckIntent",
                Signals: new List<IntentSignal>(),
                Confidence: IntentConfidence.FromScore(0.9));

            var testPolicy = new IntentPolicy()
                .AddRule(new PolicyRule(
                    "TestRule",
                    i => i.Confidence.Level == "High",
                    PolicyDecision.Allow));

            var decision = IntentPolicyEngine.Evaluate(testIntent, testPolicy);
            if (decision == PolicyDecision.Allow)
                return Task.FromResult(HealthCheckResult.Healthy("Policy engine is healthy"));
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Policy engine returned unexpected decision: {decision}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Policy engine health check failed",
                ex));
        }
    }
}
