using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core;
using Intentum.Core.Behavior;

namespace Intentum.Tests;

/// <summary>
/// Tests for advanced similarity engines: WeightedAverage and TimeDecay.
/// </summary>
public class SimilarityEngineTests
{
    [Fact]
    public void WeightedAverageSimilarityEngine_WithWeights_AppliesWeights()
    {
        // Arrange
        var weights = new Dictionary<string, double>
        {
            { "user:login", 2.0 },      // Login is twice as important
            { "user:submit", 1.5 },      // Submit is 1.5x important
            { "user:retry", 0.5 }        // Retry is less important
        };
        var engine = new WeightedAverageSimilarityEngine(weights);

        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8),
            new("user:submit", 0.9),
            new("user:retry", 0.6)
        };

        // Act
        var score = engine.CalculateIntentScore(embeddings);

        // Assert
        // Expected: (0.8*2.0 + 0.9*1.5 + 0.6*0.5) / (2.0 + 1.5 + 0.5) = (1.6 + 1.35 + 0.3) / 4.0 = 3.25 / 4.0 = 0.8125
        Assert.Equal(0.8125, score, 4);
    }

    [Fact]
    public void WeightedAverageSimilarityEngine_WithoutWeights_UsesDefaultWeight()
    {
        // Arrange
        var engine = new WeightedAverageSimilarityEngine();

        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8),
            new("user:submit", 0.9)
        };

        // Act
        var score = engine.CalculateIntentScore(embeddings);

        // Assert - Should behave like SimpleAverageSimilarityEngine
        Assert.Equal(0.85, score, 2);
    }

    [Fact]
    public void WeightedAverageSimilarityEngine_EmptyEmbeddings_ReturnsZero()
    {
        // Arrange
        var engine = new WeightedAverageSimilarityEngine();
        var embeddings = new List<IntentEmbedding>();

        // Act
        var score = engine.CalculateIntentScore(embeddings);

        // Assert
        Assert.Equal(0, score);
    }

    [Fact]
    public void WeightedAverageSimilarityEngine_CustomDefaultWeight_AppliesToUnknownSources()
    {
        // Arrange
        var weights = new Dictionary<string, double>
        {
            { "user:login", 2.0 }
        };
        var engine = new WeightedAverageSimilarityEngine(weights, defaultWeight: 0.5);

        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8),
            new("user:unknown", 0.6)  // Uses defaultWeight 0.5
        };

        // Act
        var score = engine.CalculateIntentScore(embeddings);

        // Assert
        // Expected: (0.8*2.0 + 0.6*0.5) / (2.0 + 0.5) = (1.6 + 0.3) / 2.5 = 1.9 / 2.5 = 0.76
        Assert.Equal(0.76, score, 2);
    }

    [Fact]
    public void TimeDecaySimilarityEngine_RecentEvents_HaveHigherWeight()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var halfLife = TimeSpan.FromHours(1);
        var engine = new TimeDecaySimilarityEngine(halfLife, now);

        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", now - TimeSpan.FromMinutes(10)));      // Recent
        space.Observe(new BehaviorEvent("user", "submit", now - TimeSpan.FromHours(2)));        // Older

        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8),
            new("user:submit", 0.9)
        };

        // Act
        var score = engine.CalculateIntentScoreWithTimeDecay(space, embeddings);

        // Assert
        // Recent event (10 min ago) should have higher weight than older event (2 hours ago)
        // login decay: 2^(-10/60) ≈ 0.89, submit decay: 2^(-120/60) = 2^(-2) = 0.25
        // Expected: (0.8*0.89 + 0.9*0.25) / (0.89 + 0.25) ≈ (0.712 + 0.225) / 1.14 ≈ 0.82
        Assert.True(score > 0.7); // Recent event should dominate
        Assert.True(score < 0.9); // But not completely
    }

    [Fact]
    public void TimeDecaySimilarityEngine_CurrentTimeEvents_HaveFullWeight()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var engine = new TimeDecaySimilarityEngine(TimeSpan.FromHours(1), now);

        var space = new BehaviorSpace();
        space.Observe(new BehaviorEvent("user", "login", now));  // Current time

        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8)
        };

        // Act
        var score = engine.CalculateIntentScoreWithTimeDecay(space, embeddings);

        // Assert
        // Current time should have decay factor of 1.0
        Assert.Equal(0.8, score, 2);
    }

    [Fact]
    public void TimeDecaySimilarityEngine_EmptySpace_ReturnsZero()
    {
        // Arrange
        var engine = new TimeDecaySimilarityEngine();
        var space = new BehaviorSpace();
        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8)
        };

        // Act
        var score = engine.CalculateIntentScoreWithTimeDecay(space, embeddings);

        // Assert
        Assert.Equal(0, score);
    }

    [Fact]
    public void TimeDecaySimilarityEngine_StandardInterface_UsesSimpleAverage()
    {
        // Arrange
        var engine = new TimeDecaySimilarityEngine();
        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8),
            new("user:submit", 0.9)
        };

        // Act
        var score = engine.CalculateIntentScore(embeddings);

        // Assert - Should fallback to simple average when timestamps not available
        Assert.Equal(0.85, score, 2);
    }

    [Fact]
    public void WeightedAverageSimilarityEngine_WithLlmIntentModel_WorksCorrectly()
    {
        // Arrange
        var weights = new Dictionary<string, double>
        {
            { "user:login", 2.0 },
            { "user:submit", 1.5 }
        };
        var model = new LlmIntentModel(
            new MockEmbeddingProvider(),
            new WeightedAverageSimilarityEngine(weights));

        var space = new BehaviorSpace()
            .Observe("user", "login")
            .Observe("user", "submit");

        // Act
        var intent = model.Infer(space);

        // Assert
        Assert.NotNull(intent);
        Assert.NotEmpty(intent.Signals);
        // Weighted average should produce a different confidence than simple average
        Assert.True(intent.Confidence.Score >= 0 && intent.Confidence.Score <= 1);
    }

    [Fact]
    public void CosineSimilarityEngine_WithVectors_CalculatesCosineSimilarity()
    {
        // Arrange
        var engine = new CosineSimilarityEngine();

        // Create embeddings with vectors
        var vector1 = new List<double> { 1.0, 0.0, 0.0 }; // Unit vector along x-axis
        var vector2 = new List<double> { 0.0, 1.0, 0.0 }; // Unit vector along y-axis (orthogonal)
        var vector3 = new List<double> { 1.0, 0.0, 0.0 }; // Same as vector1

        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8, vector1),
            new("user:submit", 0.9, vector2),
            new("user:retry", 0.7, vector3)
        };

        // Act
        var score = engine.CalculateIntentScore(embeddings);

        // Assert
        // vector1 and vector3 are identical (cosine = 1), vector1 and vector2 are orthogonal (cosine = 0)
        // After normalization to [0,1]: (1 + 0.5 + 0.5) / 3 = 0.67
        Assert.True(score >= 0 && score <= 1);
        Assert.True(score > 0.5); // Should be closer to 1 since two vectors are identical
    }

    [Fact]
    public void CosineSimilarityEngine_WithoutVectors_FallsBackToAverage()
    {
        // Arrange
        var engine = new CosineSimilarityEngine();

        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8),
            new("user:submit", 0.9)
        };

        // Act
        var score = engine.CalculateIntentScore(embeddings);

        // Assert - Should fallback to simple average
        Assert.Equal(0.85, score, 2);
    }

    [Fact]
    public void CosineSimilarityEngine_EmptyEmbeddings_ReturnsZero()
    {
        // Arrange
        var engine = new CosineSimilarityEngine();
        var embeddings = new List<IntentEmbedding>();

        // Act
        var score = engine.CalculateIntentScore(embeddings);

        // Assert
        Assert.Equal(0, score);
    }

    [Fact]
    public void CosineSimilarityEngine_SingleEmbedding_ReturnsScore()
    {
        // Arrange
        var engine = new CosineSimilarityEngine();
        var vector = new List<double> { 1.0, 0.0, 0.0 };
        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8, vector)
        };

        // Act
        var score = engine.CalculateIntentScore(embeddings);

        // Assert
        Assert.Equal(0.8, score, 2);
    }

    [Fact]
    public void CosineSimilarityEngine_WithLlmIntentModel_WorksCorrectly()
    {
        // Arrange
        var model = new LlmIntentModel(
            new MockEmbeddingProvider(),
            new CosineSimilarityEngine());

        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
                .Action("login")
                .Action("submit")
            .Build();

        // Act
        var intent = model.Infer(space);

        // Assert
        Assert.NotNull(intent);
        Assert.NotEmpty(intent.Signals);
        Assert.True(intent.Confidence.Score >= 0 && intent.Confidence.Score <= 1);
    }

    [Fact]
    public void CompositeSimilarityEngine_WithEqualWeights_CombinesEngines()
    {
        // Arrange
        var engine1 = new SimpleAverageSimilarityEngine();
        var engine2 = new WeightedAverageSimilarityEngine();
        IIntentSimilarityEngine[] engines = new IIntentSimilarityEngine[] { engine1, engine2 };
        var composite = new CompositeSimilarityEngine(engines);

        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8),
            new("user:submit", 0.9)
        };

        // Act
        var score = composite.CalculateIntentScore(embeddings);

        // Assert
        // Both engines should produce similar results (0.85), composite should be around 0.85
        Assert.True(score >= 0 && score <= 1);
        Assert.True(Math.Abs(score - 0.85) < 0.1);
    }

    [Fact]
    public void CompositeSimilarityEngine_WithCustomWeights_AppliesWeights()
    {
        // Arrange
        var engine1 = new SimpleAverageSimilarityEngine();
        var engine2 = new SimpleAverageSimilarityEngine(); // Same engine for testing
        var engines = new (IIntentSimilarityEngine Engine, double Weight)[]
        {
            (engine1, 2.0),
            (engine2, 1.0)
        };
        var composite = new CompositeSimilarityEngine(engines);

        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8),
            new("user:submit", 0.9)
        };

        // Act
        var score = composite.CalculateIntentScore(embeddings);

        // Assert
        // Weighted average: (0.85*2.0 + 0.85*1.0) / 3.0 = 0.85
        Assert.Equal(0.85, score, 2);
    }

    [Fact]
    public void CompositeSimilarityEngine_EmptyEngines_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new CompositeSimilarityEngine(Array.Empty<IIntentSimilarityEngine>()));
    }

    [Fact]
    public void CompositeSimilarityEngine_EmptyEmbeddings_ReturnsZero()
    {
        // Arrange
        var engines = new[] { new SimpleAverageSimilarityEngine() };
        var composite = new CompositeSimilarityEngine(engines);
        var embeddings = new List<IntentEmbedding>();

        // Act
        var score = composite.CalculateIntentScore(embeddings);

        // Assert
        Assert.Equal(0, score);
    }

    [Fact]
    public void CompositeSimilarityEngine_WithMultipleEngines_CombinesCorrectly()
    {
        // Arrange
        var weights = new Dictionary<string, double> { { "user:login", 2.0 } };
        var engine1 = new SimpleAverageSimilarityEngine();
        var engine2 = new WeightedAverageSimilarityEngine(weights);
        var engine3 = new CosineSimilarityEngine();
        var engines = new (IIntentSimilarityEngine Engine, double Weight)[]
        {
            (engine1, 1.0),
            (engine2, 1.0),
            (engine3, 1.0)
        };
        var composite = new CompositeSimilarityEngine(engines);

        var embeddings = new List<IntentEmbedding>
        {
            new("user:login", 0.8),
            new("user:submit", 0.9)
        };

        // Act
        var score = composite.CalculateIntentScore(embeddings);

        // Assert
        Assert.True(score >= 0 && score <= 1);
    }

    [Fact]
    public void CompositeSimilarityEngine_WithLlmIntentModel_WorksCorrectly()
    {
        // Arrange
        IIntentSimilarityEngine[] engines = new IIntentSimilarityEngine[]
        {
            new SimpleAverageSimilarityEngine(),
            new CosineSimilarityEngine()
        };
        var composite = new CompositeSimilarityEngine(engines);
        var model = new LlmIntentModel(
            new MockEmbeddingProvider(),
            composite);

        var space = new BehaviorSpaceBuilder()
            .WithActor("user")
                .Action("login")
                .Action("submit")
            .Build();

        // Act
        var intent = model.Infer(space);

        // Assert
        Assert.NotNull(intent);
        Assert.NotEmpty(intent.Signals);
        Assert.True(intent.Confidence.Score >= 0 && intent.Confidence.Score <= 1);
    }
}
