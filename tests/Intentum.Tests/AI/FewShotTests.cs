using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.AI.FewShot;

namespace Intentum.Tests.AI;

public sealed class FewShotTests
{
    [Fact]
    public void AddExample_StoresExample()
    {
        var store = new MemoryFewShotStore();
        store.AddExample(new FewShotExample("Purchase", ["view:product", "cart:add"], 0.9));

        var results = store.FindSimilar(["view:product", "cart:add"], topK: 3);
        Assert.Single(results);
        Assert.Equal("Purchase", results[0].IntentName);
    }

    [Fact]
    public void FindSimilar_WithNoMatch_ReturnsEmpty()
    {
        var store = new MemoryFewShotStore();
        store.AddExample(new FewShotExample("Support", ["contact:help", "ticket:open"], 0.85));

        var results = store.FindSimilar(["browse:home"], topK: 3);
        Assert.Empty(results);
    }

    [Fact]
    public void FindSimilar_ReturnsTopK()
    {
        var store = new MemoryFewShotStore();
        store.AddExample(new FewShotExample("A", ["key:1", "key:2"], 0.9));
        store.AddExample(new FewShotExample("B", ["key:3", "key:4"], 0.8));
        store.AddExample(new FewShotExample("C", ["key:5", "key:6"], 0.7));

        var results = store.FindSimilar(["key:1", "key:2", "key:3"], topK: 2);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Clear_RemovesAllExamples()
    {
        var store = new MemoryFewShotStore();
        store.AddExample(new FewShotExample("Test", ["a:b"], 0.5));
        store.Clear();

        var results = store.FindSimilar(["a:b"], topK: 1);
        Assert.Empty(results);
    }

    [Fact]
    public void FewShotIntentModel_Infer_WithMatch_ReturnsIntent()
    {
        var store = new MemoryFewShotStore();
        store.AddExample(new FewShotExample("Purchase", ["view:product", "cart:add"], 0.85));

        var model = new FewShotIntentModel(store);
        var space = new BehaviorSpace()
            .Observe("user", "view:product")
            .Observe("user", "cart:add");

        var result = model.Infer(space);

        Assert.NotNull(result);
        Assert.Equal("Purchase", result.Name);
    }

    [Fact]
    public void FewShotIntentModel_Infer_WithNoMatch_ReturnsUnknown()
    {
        var store = new MemoryFewShotStore();
        var model = new FewShotIntentModel(store);
        var space = new BehaviorSpace().Observe("user", "unknown:action");

        var result = model.Infer(space);

        Assert.NotNull(result);
        Assert.Equal("Unknown", result.Name);
    }

    [Fact]
    public void FewShotIntentModel_Infer_EmptyStore_ReturnsUnknown()
    {
        var model = new FewShotIntentModel(new MemoryFewShotStore());
        var space = new BehaviorSpace().Observe("user", "test:action");

        var result = model.Infer(space);

        Assert.Equal("Unknown", result.Name);
    }
}
