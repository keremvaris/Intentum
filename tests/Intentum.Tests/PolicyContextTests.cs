using Intentum.Core.Intents;
using Intentum.Runtime.Policy;

namespace Intentum.Tests;

public sealed class PolicyContextTests
{
    [Fact]
    public void PolicyContext_StoresIntentAndOptionalFields()
    {
        var intent = new Intent("Login", [], new IntentConfidence(0.9, "High"));
        var context = new PolicyContext(intent, SystemLoad: 0.5, Region: "EU");

        Assert.Same(intent, context.Intent);
        Assert.Equal(0.5, context.SystemLoad);
        Assert.Equal("EU", context.Region);
    }

    [Fact]
    public void PolicyContext_WithRecentIntents_StoresSummaries()
    {
        var intent = new Intent("X", [], new IntentConfidence(0.8, "High"));
        var recent = new List<IntentSummary> { new("Past", "Medium", 0.6) };
        var context = new PolicyContext(intent, RecentIntents: recent);

        Assert.NotNull(context.RecentIntents);
        Assert.Single(context.RecentIntents);
        Assert.Equal("Past", context.RecentIntents[0].Name);
        Assert.Equal(0.6, context.RecentIntents[0].ConfidenceScore);
    }

    [Fact]
    public void IntentSummary_RecordValues()
    {
        var summary = new IntentSummary("Name", "High", 0.85);
        Assert.Equal("Name", summary.Name);
        Assert.Equal("High", summary.ConfidenceLevel);
        Assert.Equal(0.85, summary.ConfidenceScore);
    }
}
