using Intentum.AI;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Tests;

/// <summary>
/// Tests for <see cref="IntentModelStreamingExtensions"/> (InferMany, InferManyAsync).
/// </summary>
public class IntentModelStreamingExtensionsTests
{
    private static LlmIntentModel CreateModel()
        => new LlmIntentModel(new MockEmbeddingProvider(), new SimpleAverageSimilarityEngine());

    [Fact]
    public void InferMany_WhenModelNull_Throws()
    {
        var spaces = new List<BehaviorSpace> { new BehaviorSpaceBuilder().WithActor("u").Action("a").Build() };
        IIntentModel? model = null;

        Assert.Throws<ArgumentNullException>(() => model!.InferMany(spaces).ToList());
    }

    [Fact]
    public void InferMany_WhenSpacesNull_Throws()
    {
        var model = CreateModel();
        IEnumerable<BehaviorSpace>? spaces = null;

        Assert.Throws<ArgumentNullException>(() => model.InferMany(spaces!).ToList());
    }

    [Fact]
    public void InferMany_EmptySpaces_ReturnsEmptySequence()
    {
        var model = CreateModel();
        var spaces = Array.Empty<BehaviorSpace>();

        var intents = model.InferMany(spaces).ToList();

        Assert.Empty(intents);
    }

    [Fact]
    public void InferMany_MultipleSpaces_ReturnsOneIntentPerSpaceInOrder()
    {
        var model = CreateModel();
        var spaces = new List<BehaviorSpace>
        {
            new BehaviorSpaceBuilder().WithActor("user").Action("login").Build(),
            new BehaviorSpaceBuilder().WithActor("user").Action("submit").Build(),
            new BehaviorSpaceBuilder().WithActor("system").Action("validate").Build()
        };

        var intents = model.InferMany(spaces).ToList();

        Assert.Equal(3, intents.Count);
        foreach (var intent in intents)
        {
            Assert.NotNull(intent.Name);
            Assert.NotNull(intent.Confidence);
            Assert.NotNull(intent.Signals);
        }
    }

    [Fact]
    public void InferMany_IsLazy_DoesNotEnumerateUntilConsumed()
    {
        var model = CreateModel();
        var enumerated = false;
        IEnumerable<BehaviorSpace> spaces = Enumerable.Range(0, 1).Select(_ =>
        {
            enumerated = true;
            return new BehaviorSpaceBuilder().WithActor("u").Action("a").Build();
        });

        var sequence = model.InferMany(spaces);
        Assert.False(enumerated);

        _ = sequence.ToList();
        Assert.True(enumerated);
    }

    [Fact]
    public async Task InferManyAsync_WhenModelNull_Throws()
    {
        var spaces = AsyncEnumerable.Empty<BehaviorSpace>();
        IIntentModel? model = null;

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await foreach (var _ in model!.InferManyAsync(spaces)) { }
        });
    }

    [Fact]
    public async Task InferManyAsync_WhenSpacesNull_Throws()
    {
        var model = CreateModel();
        IAsyncEnumerable<BehaviorSpace>? spaces = null;

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await foreach (var _ in model.InferManyAsync(spaces!)) { }
        });
    }

    [Fact]
    public async Task InferManyAsync_EmptySpaces_ReturnsEmptySequence()
    {
        var model = CreateModel();
        var spaces = AsyncEnumerable.Empty<BehaviorSpace>();

        var count = 0;
        await foreach (var _ in model.InferManyAsync(spaces))
            count++;

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task InferManyAsync_MultipleSpaces_ReturnsOneIntentPerSpaceInOrder()
    {
        var model = CreateModel();
        var spaces = new List<BehaviorSpace>
        {
            new BehaviorSpaceBuilder().WithActor("user").Action("login").Build(),
            new BehaviorSpaceBuilder().WithActor("user").Action("submit").Build()
        };
        var asyncSpaces = ToAsyncEnumerable(spaces);

        var intents = new List<Intent>();
        await foreach (var intent in model.InferManyAsync(asyncSpaces))
            intents.Add(intent);

        Assert.Equal(2, intents.Count);
        foreach (var intent in intents)
        {
            Assert.NotNull(intent.Name);
            Assert.NotNull(intent.Confidence);
        }
    }

    [Fact]
    public async Task InferManyAsync_WithCancellation_CanBeCanceled()
    {
        var model = CreateModel();
        using var cts = new CancellationTokenSource();
        var spaces = InfiniteAsyncSpaces();

        cts.CancelAfter(50);

        try
        {
            await foreach (var _ in model.InferManyAsync(spaces, cts.Token))
                await Task.Delay(10, cts.Token);
            Assert.Fail("Expected cancellation.");
        }
        catch (Exception ex)
        {
            Assert.IsType<OperationCanceledException>(ex, exactMatch: false);
        }
    }

    private static async IAsyncEnumerable<BehaviorSpace> ToAsyncEnumerable(List<BehaviorSpace> list)
    {
        foreach (var s in list)
        {
            await Task.Yield();
            yield return s;
        }
    }

    // Infinite sequence by design for cancellation test.
    // ReSharper disable once IteratorNeverReturns
    private static async IAsyncEnumerable<BehaviorSpace> InfiniteAsyncSpaces()
    {
        var space = new BehaviorSpaceBuilder().WithActor("u").Action("a").Build();
        while (true)
        {
            await Task.Yield();
            yield return space;
        }
    }
}
