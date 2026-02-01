using Intentum.Core.Contracts;

namespace Intentum.Sample.Blazor.Api;

internal sealed class PlaygroundModelRegistry : IPlaygroundModelRegistry
{
    private readonly IReadOnlyDictionary<string, IIntentModel> _models;

    public PlaygroundModelRegistry(IReadOnlyDictionary<string, IIntentModel> models)
    {
        _models = models ?? throw new ArgumentNullException(nameof(models));
    }

    public IReadOnlyList<string> GetModelNames() => _models.Keys.ToList();

    public bool TryGetModel(string name, out IIntentModel? model) => _models.TryGetValue(name, out model);
}
