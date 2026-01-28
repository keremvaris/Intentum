using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

/// <summary>
/// Workflow process status tests: Draft → InProgress → UnderReview → Approved / Rejected → Completed.
/// Transitions typical for ICMA, LMA, Sukuk, EU Green Bond and other complex workflows.
/// </summary>
public class WorkflowStatusTests
{
    private static LlmIntentModel CreateModel()
    {
        return new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
    }

    [Fact]
    public void ProcessStatus_DraftToInProgress_ProducesValidDecision()
    {
        var space = new BehaviorSpace()
            .Observe("process", "Draft")
            .Observe("process", "InProgress");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "RejectedBlock",
                i => i.Signals.Any(s => s.Description.Contains("Rejected", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "CompletedAllow",
                i => i.Signals.Any(s => s.Description.Contains("Completed", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Allow))
            .AddRule(new PolicyRule(
                "HighConfidenceAllow",
                i => i.Confidence.Level is "High" or "Certain",
                PolicyDecision.Allow))
            .AddRule(new PolicyRule("DefaultObserve", _ => true, PolicyDecision.Observe));

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Allow, PolicyDecision.Observe });
        Assert.Equal(2, space.Events.Count);
    }

    [Fact]
    public void ProcessStatus_DraftToInProgressToUnderReviewToApproved_ProducesValidDecision()
    {
        var space = new BehaviorSpace()
            .Observe("process", "Draft")
            .Observe("process", "InProgress")
            .Observe("process", "UnderReview")
            .Observe("process", "Approved");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "RejectedBlock",
                i => i.Signals.Any(s => s.Description.Contains("Rejected", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "ApprovedAllow",
                i => i.Signals.Any(s => s.Description.Contains("Approved", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Allow))
            .AddRule(new PolicyRule("DefaultObserve", _ => true, PolicyDecision.Observe));

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Equal(PolicyDecision.Allow, decision);
        Assert.Equal(4, space.Events.Count);
    }

    [Fact]
    public void ProcessStatus_DraftToInProgressToUnderReviewToApprovedToCompleted_ProducesAllow()
    {
        var space = new BehaviorSpace()
            .Observe("process", "Draft")
            .Observe("process", "InProgress")
            .Observe("process", "UnderReview")
            .Observe("process", "Approved")
            .Observe("process", "Completed");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "RejectedBlock",
                i => i.Signals.Any(s => s.Description.Contains("Rejected", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "CompletedAllow",
                i => i.Signals.Any(s => s.Description.Contains("Completed", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Allow))
            .AddRule(new PolicyRule(
                "ApprovedAllow",
                i => i.Signals.Any(s => s.Description.Contains("Approved", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Allow))
            .AddRule(new PolicyRule("DefaultObserve", _ => true, PolicyDecision.Observe));

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Equal(PolicyDecision.Allow, decision);
        Assert.Equal(5, space.Events.Count);
    }

    [Fact]
    public void ProcessStatus_DraftToInProgressToUnderReviewToRejected_ProducesBlock()
    {
        var space = new BehaviorSpace()
            .Observe("process", "Draft")
            .Observe("process", "InProgress")
            .Observe("process", "UnderReview")
            .Observe("process", "Rejected");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "RejectedBlock",
                i => i.Signals.Any(s => s.Description.Contains("Rejected", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "ApprovedAllow",
                i => i.Signals.Any(s => s.Description.Contains("Approved", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Allow))
            .AddRule(new PolicyRule("DefaultObserve", _ => true, PolicyDecision.Observe));

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Equal(PolicyDecision.Block, decision);
        Assert.Equal(4, space.Events.Count);
    }

    [Fact]
    public void ProcessStatus_StuckInDraftInProgress_ProducesObserveOrAllow()
    {
        var space = new BehaviorSpace()
            .Observe("process", "Draft")
            .Observe("process", "InProgress");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "RejectedBlock",
                i => i.Signals.Any(s => s.Description.Contains("Rejected", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "HighConfidenceAllow",
                i => i.Confidence.Level is "High" or "Certain",
                PolicyDecision.Allow))
            .AddRule(new PolicyRule("DefaultObserve", _ => true, PolicyDecision.Observe));

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Contains(decision, new[] { PolicyDecision.Allow, PolicyDecision.Observe });
    }

    [Fact]
    public void ProcessStatus_EUGreenBondStyle_UnderReviewToApprovedToCompleted_ProducesAllow()
    {
        var space = new BehaviorSpace()
            .Observe("process", "UnderReview")
            .Observe("process", "Approved")
            .Observe("process", "Completed");
        var model = CreateModel();
        var policy = new IntentPolicy()
            .AddRule(new PolicyRule(
                "RejectedBlock",
                i => i.Signals.Any(s => s.Description.Contains("Rejected", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Block))
            .AddRule(new PolicyRule(
                "CompletedAllow",
                i => i.Signals.Any(s => s.Description.Contains("Completed", StringComparison.OrdinalIgnoreCase)),
                PolicyDecision.Allow))
            .AddRule(new PolicyRule("DefaultObserve", _ => true, PolicyDecision.Observe));

        var intent = model.Infer(space);
        var decision = intent.Decide(policy);

        Assert.Equal(PolicyDecision.Allow, decision);
    }
}
