using Intentum.Sample.Blazor;
using Microsoft.AspNetCore.Http;

namespace Intentum.Sample.Blazor.Tests.SystemOne;

// Shares a collection with SystemOneEndpointsTests: these tests mutate process-wide
// env vars (SYSTEMONE_BASE_URL / SYSTEMONE_API_KEY) that the infer route reads, so the
// two classes must NOT run in parallel (xUnit parallelizes across classes by default).
[Collection("SystemOneEnv")]
public sealed class SystemOneOptionsResolutionTests
{
    private static HttpContext Ctx()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Scheme = "http";
        ctx.Request.Host = new HostString("localhost", 5177);
        return ctx;
    }

    [Fact]
    public void Resolve_ApiKeyFromBodyWinsOverEnvironment()
    {
        Environment.SetEnvironmentVariable("SYSTEMONE_API_KEY", "env-key");
        try
        {
            var options = ProgramConfiguration.ResolveSystemOneOptions("jev", null, "body-key", Ctx());

            Assert.Equal("body-key", options.ApiKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SYSTEMONE_API_KEY", null);
        }
    }

    [Fact]
    public void Resolve_ApiKeyFallsBackToEnvironment_WhenBodyEmpty()
    {
        Environment.SetEnvironmentVariable("SYSTEMONE_API_KEY", "env-key");
        try
        {
            var options = ProgramConfiguration.ResolveSystemOneOptions("jev", null, null, Ctx());

            Assert.Equal("env-key", options.ApiKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SYSTEMONE_API_KEY", null);
        }
    }

    [Fact]
    public void Resolve_EmptyApiKey_FallsBackToEnvironment()
    {
        Environment.SetEnvironmentVariable("SYSTEMONE_API_KEY", "env-key");
        try
        {
            var options = ProgramConfiguration.ResolveSystemOneOptions("jev", null, "", Ctx());

            Assert.Equal("env-key", options.ApiKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SYSTEMONE_API_KEY", null);
        }
    }

    [Fact]
    public void Resolve_BaseUrlFromEnvironment_UsedWhenQueryEmpty()
    {
        Environment.SetEnvironmentVariable("SYSTEMONE_BASE_URL", "http://engine.internal:9000");
        try
        {
            var options = ProgramConfiguration.ResolveSystemOneOptions("kev", null, null, Ctx());

            Assert.Equal("http://engine.internal:9000", options.BaseUrl);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SYSTEMONE_BASE_URL", null);
        }
    }

    [Fact]
    public void Resolve_QueryBaseUrl_WinsOverEnvironmentAndPreset()
    {
        Environment.SetEnvironmentVariable("SYSTEMONE_BASE_URL", "http://env:1");
        try
        {
            var options = ProgramConfiguration.ResolveSystemOneOptions("kev", "http://query:2", null, Ctx());

            Assert.Equal("http://query:2", options.BaseUrl);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SYSTEMONE_BASE_URL", null);
        }
    }

    [Fact]
    public void Resolve_DemoWithoutAnyBaseUrl_UsesRequestOrigin()
    {
        Environment.SetEnvironmentVariable("SYSTEMONE_BASE_URL", null);
        Environment.SetEnvironmentVariable("SYSTEMONE_API_KEY", null);
        var options = ProgramConfiguration.ResolveSystemOneOptions("demo", null, null, Ctx());

        Assert.Equal("http://localhost:5177", options.BaseUrl);
        Assert.Equal("demo", options.Engine);
        Assert.Null(options.ApiKey);
    }

    [Fact]
    public void Resolve_DefaultEngine_IsDemo_AndPresetsKeepUrls()
    {
        Environment.SetEnvironmentVariable("SYSTEMONE_BASE_URL", null);
        Environment.SetEnvironmentVariable("SYSTEMONE_API_KEY", null);
        Environment.SetEnvironmentVariable("SYSTEMONE_ENGINE", null);
        var demo = ProgramConfiguration.ResolveSystemOneOptions(null, null, null, Ctx());
        var kev = ProgramConfiguration.ResolveSystemOneOptions("kev", null, null, Ctx());

        Assert.Equal("demo", demo.Engine);
        Assert.Equal("http://127.0.0.1:8009", kev.BaseUrl);
    }

    [Fact]
    public void Resolve_EngineFromEnvironment_UsedWhenQueryNull()
    {
        Environment.SetEnvironmentVariable("SYSTEMONE_BASE_URL", null);
        Environment.SetEnvironmentVariable("SYSTEMONE_ENGINE", "kev");
        try
        {
            var options = ProgramConfiguration.ResolveSystemOneOptions(null, null, null, Ctx());

            Assert.Equal("kev", options.Engine);
            Assert.Equal("http://127.0.0.1:8009", options.BaseUrl);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SYSTEMONE_ENGINE", null);
        }
    }

    [Fact]
    public void Resolve_UnknownEngine_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            ProgramConfiguration.ResolveSystemOneOptions("nope", null, null, Ctx()));
    }

    [Fact]
    public void Resolve_MalformedBaseUrl_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            ProgramConfiguration.ResolveSystemOneOptions("kev", "not-a-url", null, Ctx()));
    }
}
