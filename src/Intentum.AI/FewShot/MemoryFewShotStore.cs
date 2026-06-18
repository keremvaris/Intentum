namespace Intentum.AI.FewShot;

public sealed class MemoryFewShotStore : IFewShotStore
{
    private readonly List<FewShotExample> _examples = new();

    public int Count => _examples.Count;

    public void AddExample(FewShotExample example)
    {
        _examples.Add(example);
    }

    public IReadOnlyList<FewShotExample> FindSimilar(string[] behaviorKeys, int topK = 5)
    {
        var scored = _examples
            .Select(e => (Example: e, Overlap: e.BehaviorKeys.Intersect(behaviorKeys).Count()))
            .Where(x => x.Overlap > 0)
            .OrderByDescending(x => x.Overlap)
            .ThenByDescending(x => x.Example.Confidence)
            .Take(topK)
            .Select(x => x.Example)
            .ToList();

        return scored;
    }

    public void Clear() => _examples.Clear();
}
