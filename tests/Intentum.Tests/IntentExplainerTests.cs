using Intentum.Core.Intents;
using Intentum.Explainability;

namespace Intentum.Tests;

public class IntentExplainerTests
{
    [Fact]
    public void GetSignalContributions_EmptySignals_ReturnsEmpty()
    {
        var intent = new Intent("Test", Array.Empty<IntentSignal>(), new IntentConfidence(0.5, "Medium"));
        var explainer = new IntentExplainer();
        var contributions = explainer.GetSignalContributions(intent);
        Assert.Empty(contributions);
    }

    [Fact]
    public void GetSignalContributions_WithSignals_ReturnsContributions()
    {
        var signals = new List<IntentSignal>
        {
            new("a", "desc1", 2.0),
            new("b", "desc2", 1.0),
            new("c", "desc3", 1.0)
        };
        var intent = new Intent("Test", signals, new IntentConfidence(0.7, "High"));
        var explainer = new IntentExplainer();
        var contributions = explainer.GetSignalContributions(intent);
        Assert.Equal(3, contributions.Count);
        Assert.True(contributions[0].ContributionPercent > contributions[1].ContributionPercent);
        var total = contributions.Sum(c => c.ContributionPercent);
        Assert.True(Math.Abs(total - 100) < 0.01);
    }

    [Fact]
    public void GetExplanation_ReturnsHumanReadableText()
    {
        var signals = new List<IntentSignal> { new("user:login", "login", 1.0) };
        var intent = new Intent("Login", signals, new IntentConfidence(0.8, "High"));
        var explainer = new IntentExplainer();
        var text = explainer.GetExplanation(intent, maxSignals: 3);
        Assert.Contains("Login", text);
        Assert.Contains("High", text);
    }
}
