using Intentum.Core.Intents;

namespace Intentum.AI.Ensemble;

public sealed record ModelResult(string Name, double Score, double Weight);

public interface IEnsembleStrategy
{
    Intent Combine(IReadOnlyList<ModelResult> results);
}
