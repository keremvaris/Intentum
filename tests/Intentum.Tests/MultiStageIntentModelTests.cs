using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;
using Intentum.Core.Models;

namespace Intentum.Tests;

public sealed class MultiStageIntentModelTests
{
    [Fact]
    public void Constructor_WhenStagesNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MultiStageIntentModel(null!));
    }

    [Fact]
    public void Constructor_WhenStagesEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MultiStageIntentModel([]));
    }

    [Fact]
    public void Infer_WhenFirstStageAboveThreshold_ReturnsFirstStageResult()
    {
        var highIntent = new Intent("High", [], IntentConfidence.FromScore(0.9), "rule");
        var lowIntent = new Intent("Low", [], IntentConfidence.FromScore(0.3), "fallback");
        var stage1 = new StubIntentModel(highIntent);
        var stage2 = new StubIntentModel(lowIntent);
        var model = new MultiStageIntentModel([(stage1, 0.8), (stage2, 0.0)]);

        var space = new BehaviorSpace();
        var result = model.Infer(space);

        Assert.Equal("High", result.Name);
        Assert.Equal(0.9, result.Confidence.Score);
    }

    [Fact]
    public void Infer_WhenFirstStageBelowThreshold_ReturnsSecondStageResult()
    {
        var lowIntent = new Intent("Low", [], IntentConfidence.FromScore(0.5), "stage1");
        var highIntent = new Intent("High", [], IntentConfidence.FromScore(0.95), "stage2");
        var stage1 = new StubIntentModel(lowIntent);
        var stage2 = new StubIntentModel(highIntent);
        var model = new MultiStageIntentModel([(stage1, 0.8), (stage2, 0.0)]);

        var space = new BehaviorSpace();
        var result = model.Infer(space);

        Assert.Equal("High", result.Name);
        Assert.Equal(0.95, result.Confidence.Score);
    }

    [Fact]
    public void Infer_WhenNoStageMeetsThreshold_ReturnsLastStageWithReasoning()
    {
        var low1 = new Intent("A", [], IntentConfidence.FromScore(0.3));
        var low2 = new Intent("B", [], IntentConfidence.FromScore(0.4));
        var model = new MultiStageIntentModel([
            (new StubIntentModel(low1), 0.9),
            (new StubIntentModel(low2), 0.9)
        ]);

        var space = new BehaviorSpace();
        var result = model.Infer(space);

        Assert.Equal("B", result.Name);
        Assert.Contains("last stage", result.Reasoning!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Infer_ClampsThresholdBetweenZeroAndOne()
    {
        var intent = new Intent("X", [], IntentConfidence.FromScore(0.5));
        var stage = new StubIntentModel(intent);
        var model = new MultiStageIntentModel([(stage, 1.5)]); // clamped to 1

        var result = model.Infer(new BehaviorSpace());
        Assert.Equal("X", result.Name);
    }

    private sealed class StubIntentModel : IIntentModel
    {
        private readonly Intent _intent;

        public StubIntentModel(Intent intent) => _intent = intent;

        public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null) => _intent;
    }
}
