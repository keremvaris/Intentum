// Intentum.Providers is a meta-package. It has no public API;
// it only references Intentum.Core, Intentum.Runtime, Intentum.AI,
// and all AI provider packages (OpenAI, Gemini, Mistral, Azure OpenAI, Claude).
// Adding this package brings in the full set of provider adapters.

namespace Intentum.Providers;

internal static class MetaPackage { }
