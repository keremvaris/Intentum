using Intentum.Core.Intents;

namespace Intentum.Core.Evaluation;

public sealed record IntentEvaluationResult(
    Intent Intent,
    Behavior.BehaviorVector BehaviorVector
);
