namespace Intentum.CodeGen;

public static class ScaffoldRunner
{
    private const string DefaultNamespace = "Intentum.Cqrs.Web";

    public static int Run(DirectoryInfo output)
    {
        output.Create();
        var root = output.FullName;

        WriteProject(root);
        WriteProgram(root);
        WriteFeaturesSample(root);
        WriteBehaviors(root);

        Console.WriteLine($"Scaffold written to: {root}");
        return 0;
    }

    private static void WriteProject(string root)
    {
        var path = Path.Combine(root, "Intentum.Cqrs.Web.csproj");
        if (File.Exists(path))
        {
            Console.WriteLine($"Skipping existing: {path}");
            return;
        }
        File.WriteAllText(path, """
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.11.0" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
    <PackageReference Include="MediatR" Version="12.4.1" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.2" />
    <PackageReference Include="Scalar.AspNetCore" Version="2.12.20" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Intentum.Core" Version="1.0.0-alpha.0.1" />
    <PackageReference Include="Intentum.Runtime" Version="1.0.0-alpha.0.1" />
    <PackageReference Include="Intentum.AI" Version="1.0.0-alpha.0.1" />
  </ItemGroup>
</Project>
""");
    }

    private static void WriteProgram(string root)
    {
        var path = Path.Combine(root, "Program.cs");
        if (File.Exists(path))
        {
            Console.WriteLine($"Skipping existing: {path}");
            return;
        }
        var programContent = $@"using System.Reflection;
using FluentValidation;
using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.Core.Contracts;
using Intentum.Runtime.Policy;
using MediatR;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof({DefaultNamespace}.Behaviors.ValidationBehavior<,>));
builder.Services.AddSingleton<IIntentEmbeddingProvider, MockEmbeddingProvider>();
builder.Services.AddSingleton<IIntentSimilarityEngine, SimpleAverageSimilarityEngine>();
builder.Services.AddSingleton<IIntentModel>(sp =>
{{
    var e = sp.GetRequiredService<IIntentEmbeddingProvider>();
    var s = sp.GetRequiredService<IIntentSimilarityEngine>();
    return new LlmIntentModel(e, s);
}});
builder.Services.AddSingleton(_ => new IntentPolicy()
    .AddRule(new PolicyRule(""HighConfidenceAllow"", i => i.Confidence.Level is ""High"" or ""Certain"", PolicyDecision.Allow))
    .AddRule(new PolicyRule(""MediumConfidenceObserve"", i => i.Confidence.Level == ""Medium"", PolicyDecision.Observe))
    .AddRule(new PolicyRule(""LowConfidenceWarn"", i => i.Confidence.Level == ""Low"", PolicyDecision.Warn)));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet(""/"", () => ""Intentum CQRS + Intentum sample. See /scalar for API docs."");
app.Run();
public partial class Program;
";
        File.WriteAllText(path, programContent);
    }

    private static void WriteBehaviors(string root)
    {
        var dir = Path.Combine(root, "Behaviors");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "ValidationBehavior.cs");
        if (File.Exists(path)) return;
        var behaviorContent = $@"using FluentValidation;
using MediatR;

namespace {DefaultNamespace}.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {{
        if (!_validators.Any()) return await next();
        var context = new ValidationContext<TRequest>(request);
        var failures = _validators.Select(v => v.Validate(context)).SelectMany(r => r.Errors).Where(f => f != null).ToList();
        if (failures.Count != 0) throw new ValidationException(failures);
        return await next();
    }}
}}
";
        File.WriteAllText(path, behaviorContent);
    }

    private static void WriteFeaturesSample(string root)
    {
        var featuresDir = Path.Combine(root, "Features");
        Directory.CreateDirectory(featuresDir);
        var sampleDir = Path.Combine(featuresDir, "SampleFeature");
        var commandsDir = Path.Combine(sampleDir, "Commands");
        var validatorsDir = Path.Combine(sampleDir, "Validators");
        Directory.CreateDirectory(commandsDir);
        Directory.CreateDirectory(validatorsDir);

        WriteIfMissing(Path.Combine(commandsDir, "SampleCommand.cs"), $@"using MediatR;

namespace {DefaultNamespace}.Features.SampleFeature.Commands;

public sealed record SampleCommand(string Name) : IRequest<SampleResult>;
public sealed record SampleResult(string Id, string Name);
");
        WriteIfMissing(Path.Combine(commandsDir, "SampleCommandHandler.cs"), $@"using MediatR;

namespace {DefaultNamespace}.Features.SampleFeature.Commands;

public sealed class SampleCommandHandler : IRequestHandler<SampleCommand, SampleResult>
{{
    public Task<SampleResult> Handle(SampleCommand request, CancellationToken ct)
        => Task.FromResult(new SampleResult(Guid.NewGuid().ToString(""N"")[..8], request.Name));
}}
");
        WriteIfMissing(Path.Combine(validatorsDir, "SampleCommandValidator.cs"), $@"using FluentValidation;
using {DefaultNamespace}.Features.SampleFeature.Commands;

namespace {DefaultNamespace}.Features.SampleFeature.Validators;

public sealed class SampleCommandValidator : AbstractValidator<SampleCommand>
{{
    public SampleCommandValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
}}
");
    }

    private static void WriteIfMissing(string path, string content)
    {
        if (File.Exists(path)) return;
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, content);
    }
}
