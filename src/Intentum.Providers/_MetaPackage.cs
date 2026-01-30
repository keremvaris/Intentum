namespace Intentum.Providers;

/// <summary>
/// Meta-package marker. This assembly references Intentum.Core, Intentum.Runtime, Intentum.AI,
/// and all AI provider packages (OpenAI, Gemini, Mistral, Azure OpenAI, Claude).
/// Adding this package brings in the full set of provider adapters.
/// </summary>
internal static class MetaPackage
{
    /// <summary>Package identifier for Intentum.Providers.</summary>
    public const string PackageId = "Intentum.Providers";
}
