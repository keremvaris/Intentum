using System.Runtime.CompilerServices;
using DotNetEnv;

namespace Intentum.Tests;

/// <summary>
/// Loads .env from repo root (or current/parent dirs) so OPENAI_API_KEY etc. are available
/// when running tests from IDE or dotnet test without the shell script.
/// </summary>
internal static class EnvLoader
{
    [ModuleInitializer]
    public static void LoadEnv()
    {
        Env.TraversePath().Load();
    }
}
