namespace Intentum.AI.FewShot;

public sealed record FewShotExample(
    string IntentName,
    IReadOnlyList<string> BehaviorKeys,
    double Confidence);

public interface IFewShotStore
{
    void AddExample(FewShotExample example);
    IReadOnlyList<FewShotExample> FindSimilar(string[] behaviorKeys, int topK = 5);
    void Clear();
    int Count { get; }
}
