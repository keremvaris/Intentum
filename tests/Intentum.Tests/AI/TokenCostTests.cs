using Intentum.AI.TokenCost;

namespace Intentum.Tests.AI;

public sealed class TokenCostTests
{
    [Fact]
    public void SimpleTokenCounter_CountsWords()
    {
        var counter = new SimpleTokenCounter();
        var count = counter.Count("Hello world");
        Assert.Equal(2, count);
    }

    [Fact]
    public void SimpleTokenCounter_EmptyString_ReturnsZero()
    {
        var counter = new SimpleTokenCounter();
        Assert.Equal(0, counter.Count(""));
    }

    [Fact]
    public void SimpleTokenCounter_Null_ReturnsZero()
    {
        var counter = new SimpleTokenCounter();
        Assert.Equal(0, counter.Count(null!));
    }

    [Fact]
    public void Track_AccumulatesCost()
    {
        var tracker = new MemoryTokenCostTracker();
        tracker.Track(new TokenCost("gpt-4", 100, 50, 0.01m));

        var total = tracker.GetTotal();
        Assert.Equal(100, total.PromptTokens);
        Assert.Equal(50, total.CompletionTokens);
        Assert.Equal(0.01m, total.Cost);
    }

    [Fact]
    public void Track_MultipleCalls_Accumulates()
    {
        var tracker = new MemoryTokenCostTracker();
        tracker.Track(new TokenCost("gpt-4", 100, 50, 0.01m));
        tracker.Track(new TokenCost("gpt-4", 200, 100, 0.02m));

        var total = tracker.GetTotal();
        Assert.Equal(300, total.PromptTokens);
        Assert.Equal(150, total.CompletionTokens);
        Assert.Equal(0.03m, total.Cost);
    }

    [Fact]
    public void Reset_ClearsAll()
    {
        var tracker = new MemoryTokenCostTracker();
        tracker.Track(new TokenCost("gpt-4", 100, 50, 0.01m));
        tracker.Reset();

        var total = tracker.GetTotal();
        Assert.Equal(0, total.PromptTokens);
        Assert.Equal(0, total.CompletionTokens);
        Assert.Equal(0, total.Cost);
    }

    [Fact]
    public void Track_WithDifferentModels_AccumulatesSeparately()
    {
        var tracker = new MemoryTokenCostTracker();
        tracker.Track(new TokenCost("gpt-4", 100, 50, 0.01m));
        tracker.Track(new TokenCost("claude-3", 200, 100, 0.03m));

        var gpt4 = tracker.GetTotal("gpt-4");
        Assert.Equal(100, gpt4.PromptTokens);

        var claude = tracker.GetTotal("claude-3");
        Assert.Equal(200, claude.PromptTokens);

        var all = tracker.GetTotal();
        Assert.Equal(300, all.PromptTokens);
    }
}
