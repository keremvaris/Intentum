using Intentum.Core.Contracts;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Registry of named intent models for playground compare.
/// </summary>
internal interface IPlaygroundModelRegistry
{
    IReadOnlyList<string> GetModelNames();
    bool TryGetModel(string name, out IIntentModel? model);
}
