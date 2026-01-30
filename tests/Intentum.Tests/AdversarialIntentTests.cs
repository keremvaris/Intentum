using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Chaos / adversarial tests: malicious or anomaly event sequences.
/// Verifies that the policy returns consistent Block/Observe (or Allow) for "attacker" scenarios.
/// </summary>
public class AdversarialIntentTests
{
    private static LlmIntentModel CreateModel()
    {
        return new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
    }

    /// <summary>
    /// Policy that blocks excessive retries and many failed logins (credential stuffing / brute-force style).
    /// </summary>
    private static IntentPolicy CreateAdversarialPolicy()
    {
        return new IntentPolicy()
            .AddRule(new PolicyRule(
                "BlockExcessiveRetry",
                i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "BlockManyFailedLogins",
                i => i.Signals.Count(s => s.Description.Contains("login.failed", StringComparison.OrdinalIgnoreCase)) >= 5,
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "BlockSuspiciousVelocity",
                i => i.Signals.Count >= 15,
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "HighConfidenceAllow",
                i => i.Confidence.Level is "High" or "Certain",
                PolicyDecision.Allow))
            .AddRule(new PolicyRule(
                "DefaultObserve",
                _ => true,
                PolicyDecision.Observe));
    }

    [Fact]
    public void Adversarial_ExcessiveRetries_ResultsInBlock()
    {
        // Policy blocks when >= 3 *signals* (dimensions) contain "retry". Use 3 distinct retry-like dimensions.
        var space = new BehaviorSpace()
            .Observe("user", "login.attempt")
            .Observe("user", "retry")
            .Observe("user", "retry.login")
            .Observe("user", "retry.submit");
        var model = CreateModel();
        var policy = CreateAdversarialPolicy();

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Equal(PolicyDecision.Block, decision);
    }

    [Fact]
    public void Adversarial_ManyFailedLogins_CredentialStuffingStyle_ResultsInBlock()
    {
        // Policy blocks when >= 5 signals contain "login.failed". Use 5+ distinct login.failed dimensions.
        var space = new BehaviorSpace();
        space.Observe("user", "login.failed");
        space.Observe("user", "login.failed.attempt1");
        space.Observe("user", "login.failed.attempt2");
        space.Observe("user", "login.failed.attempt3");
        space.Observe("user", "login.failed.attempt4");
        space.Observe("user", "login.failed.attempt5");
        space.Observe("user", "password.reset");

        var model = CreateModel();
        var policy = CreateAdversarialPolicy();

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Equal(PolicyDecision.Block, decision);
    }

    [Fact]
    public void Adversarial_MixedBenignAndSuspicious_ConsistentDecision()
    {
        var space = new BehaviorSpace()
            .Observe("user", "login")
            .Observe("user", "login.failed")
            .Observe("user", "login.failed")
            .Observe("user", "retry")
            .Observe("user", "retry")
            .Observe("user", "retry");
        var model = CreateModel();
        var policy = CreateAdversarialPolicy();

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Block, PolicyDecision.Observe, PolicyDecision.Allow });
        Assert.Equal(6, space.Events.Count);
    }

    [Fact]
    public void Adversarial_EdgeEmptySpace_DoesNotThrow()
    {
        var space = new BehaviorSpace();
        var model = CreateModel();
        var policy = CreateAdversarialPolicy();

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Observe, PolicyDecision.Allow });
    }

    [Fact]
    public void Adversarial_HighVolumeSuspiciousEvents_BlockOrObserve()
    {
        // Policy blocks when >= 15 signals (distinct dimensions). Use 15+ distinct probe actions.
        var space = new BehaviorSpace();
        for (var i = 0; i < 16; i++)
            space.Observe("bot", $"probe.endpoint.{i}");

        var model = CreateModel();
        var policy = CreateAdversarialPolicy();

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Block, PolicyDecision.Observe });
    }
}
