using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Tries catalog-based classification first; falls back to LLM when no catalog match or low score.
/// Ensures the default infer returns real intent names from the catalog when possible.
/// </summary>
internal sealed class CatalogFirstIntentModel(IIntentModel catalogModel, IIntentModel llmModel) : IIntentModel
{
    private const double CatalogMinScore = 0.5;

    private readonly IIntentModel _catalogModel = catalogModel ?? throw new ArgumentNullException(nameof(catalogModel));
    private readonly IIntentModel _llmModel = llmModel ?? throw new ArgumentNullException(nameof(llmModel));

    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {
        var catalogIntent = _catalogModel.Infer(behaviorSpace, precomputedVector);
        if (catalogIntent.Name != "Unknown" && catalogIntent.Confidence.Score >= CatalogMinScore)
            return catalogIntent;
        return _llmModel.Infer(behaviorSpace, precomputedVector);
    }
}
