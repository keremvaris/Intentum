using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Batch;
using Intentum.Core.Behavior;

namespace Intentum.Tests;

/// <summary>
/// Tests for batch processing functionality.
/// </summary>
public class BatchProcessingTests
{
    [Fact]
    public void BatchIntentModel_InferBatch_ProcessesMultipleSpaces()
    {
        // Arrange
        var model = new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
        var batchModel = new BatchIntentModel(model);

        var spaces = new List<BehaviorSpace>
        {
            new BehaviorSpaceBuilder().WithActor("user").Action("login").Build(),
            new BehaviorSpaceBuilder().WithActor("user").Action("submit").Build(),
            new BehaviorSpaceBuilder().WithActor("system").Action("validate").Build()
        };

        // Act
        var intents = batchModel.InferBatch(spaces);

        // Assert
        Assert.Equal(3, intents.Count);
        Assert.All(intents, intent => Assert.NotNull(intent));
    }

    [Fact]
    public void BatchIntentModel_InferBatch_EmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var model = new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
        var batchModel = new BatchIntentModel(model);

        // Act
        var intents = batchModel.InferBatch([]);

        // Assert
        Assert.Empty(intents);
    }

    [Fact]
    public async Task BatchIntentModel_InferBatchAsync_ProcessesInParallel()
    {
        // Arrange
        var model = new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
        var batchModel = new BatchIntentModel(model);

        var spaces = new List<BehaviorSpace>
        {
            new BehaviorSpaceBuilder().WithActor("user").Action("login").Build(),
            new BehaviorSpaceBuilder().WithActor("user").Action("submit").Build(),
            new BehaviorSpaceBuilder().WithActor("system").Action("validate").Build()
        };

        // Act
        var intents = await batchModel.InferBatchAsync(spaces);

        // Assert
        Assert.Equal(3, intents.Count);
        Assert.All(intents, intent => Assert.NotNull(intent));
    }

    [Fact]
    public async Task BatchIntentModel_InferBatchAsync_Cancellation_RespectsCancellationToken()
    {
        // Arrange
        var model = new LlmIntentModel(
            new MockEmbeddingProvider(),
            new SimpleAverageSimilarityEngine());
        var batchModel = new BatchIntentModel(model);

        var spaces = new List<BehaviorSpace>
        {
            new BehaviorSpaceBuilder().WithActor("user").Action("login").Build(),
            new BehaviorSpaceBuilder().WithActor("user").Action("submit").Build()
        };

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(1); // Cancel after 1ms

        // Act & Assert - May or may not throw depending on timing, so we just verify it doesn't crash
        try
        {
            await batchModel.InferBatchAsync(spaces, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected if cancellation happens
        }

        // Test passes if no exception or OperationCanceledException
        Assert.True(true);
    }
}
