using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.AI.Ensemble;

public sealed class EnsembleIntentModel : IIntentModel
{
    private readonly IReadOnlyList<IIntentModel> _models;
    private readonly IEnsembleStrategy _strategy;
    private readonly IReadOnlyList<double> _weights;

    public EnsembleIntentModel(
        IReadOnlyList<IIntentModel> models,
        IEnsembleStrategy strategy,
        IReadOnlyList<double>? weights = null)
    {
        _models = models;
        _strategy = strategy;
        _weights = weights ?? models.Select(_ => 1.0).ToList();
    }

    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var results = _models.Select((model, i) =>
        {
            var intent = model.Infer(behaviorSpace, precomputedVector);
            return new ModelResult(intent.Name, intent.Confidence.Score, _weights[i]);
        }).ToList();

        return _strategy.Combine(results);
    }
}
