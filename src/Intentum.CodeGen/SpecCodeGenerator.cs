using System.Reflection;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Intentum.CodeGen;

public static class SpecCodeGenerator
{
    private const string DefaultNamespace = "Intentum.Cqrs.Web";

    public static async Task<int> Run(FileInfo? spec, FileInfo? assembly, DirectoryInfo output)
    {
        if (spec is not null && assembly is not null)
        {
            Console.Error.WriteLine("Provide either --spec or --assembly, not both.");
            return 1;
        }
        if (spec is null && assembly is null)
        {
            Console.Error.WriteLine("Provide --spec <file> or --assembly <dll>.");
            return 1;
        }

        output.Create();
        var root = output.FullName;

        if (assembly is not null)
        {
            var features = ExtractFeaturesFromAssembly(assembly.FullName);
            foreach (var feature in features)
                EmitFeature(root, feature, DefaultNamespace);
            Console.WriteLine($"Generated {features.Count} feature(s) from {assembly.Name} into {root}");
        }
        else if (spec is not null)
        {
            var specModel = await LoadSpecAsync(spec.FullName);
            foreach (var feature in specModel.Features)
                EmitFeature(root, feature, specModel.Namespace ?? DefaultNamespace);
            Console.WriteLine($"Generated {specModel.Features.Count} feature(s) from {spec.Name} into {root}");
        }

        return 0;
    }

    private static List<FeatureSpec> ExtractFeaturesFromAssembly(string assemblyPath)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);
        var featureNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in assembly.GetExportedTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var hasFact = method.GetCustomAttributes().Any(a =>
                    a.GetType().FullName?.Contains("Fact") == true ||
                    a.GetType().FullName?.Contains("TestMethod") == true);
                if (!hasFact) continue;
                var match = Regex.Match(method.Name, @"^([A-Za-z][A-Za-z0-9]*)_");
                if (match.Success && match.Groups[1].Value.Length > 2)
                    featureNames.Add(match.Groups[1].Value);
            }
        }
        return featureNames.Select(name => new FeatureSpec
        {
            Name = name,
            Commands = [new CommandSpec { Name = name + "Command", Properties = [] }],
            Queries = []
        }).ToList();
    }

    private static async Task<SpecModel> LoadSpecAsync(string specPath)
    {
        var yaml = await File.ReadAllTextAsync(specPath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        var model = deserializer.Deserialize<SpecModel>(yaml);
        return model ?? new SpecModel { Features = [] };
    }

    private static void EmitFeature(string root, FeatureSpec feature, string ns)
    {
        var featureDir = Path.Combine(root, "Features", feature.Name);
        var commandsDir = Path.Combine(featureDir, "Commands");
        var queriesDir = Path.Combine(featureDir, "Queries");
        var handlersDir = Path.Combine(featureDir, "Handlers");
        var validatorsDir = Path.Combine(featureDir, "Validators");
        Directory.CreateDirectory(commandsDir);
        Directory.CreateDirectory(queriesDir);
        Directory.CreateDirectory(handlersDir);
        Directory.CreateDirectory(validatorsDir);

        foreach (var cmd in feature.Commands ?? [])
        {
            var cmdName = cmd.Name!.EndsWith("Command", StringComparison.Ordinal) ? cmd.Name : cmd.Name + "Command";
            var resultName = cmdName.Replace("Command", "Result");
            WriteIfMissing(Path.Combine(commandsDir, cmdName + ".cs"), $"""
using MediatR;

namespace {ns}.Features.{feature.Name}.Commands;

public sealed record {cmdName}({FormatProperties(cmd.Properties)}) : IRequest<{resultName}>;
public sealed record {resultName}(string Id);
""");
            WriteIfMissing(Path.Combine(handlersDir, cmdName + "Handler.cs"), $@"using MediatR;

namespace {ns}.Features.{feature.Name}.Handlers;

public sealed class {cmdName}Handler : IRequestHandler<Commands.{cmdName}, Commands.{resultName}>
{{
    public Task<Commands.{resultName}> Handle(Commands.{cmdName} request, CancellationToken ct)
        => Task.FromResult(new Commands.{resultName}(Guid.NewGuid().ToString(""N"")[..8]));
}}
");
            WriteIfMissing(Path.Combine(validatorsDir, cmdName + "Validator.cs"), $@"using FluentValidation;
using {ns}.Features.{feature.Name}.Commands;

namespace {ns}.Features.{feature.Name}.Validators;

public sealed class {cmdName}Validator : AbstractValidator<{cmdName}>
{{
    public {cmdName}Validator() {{ }}
}}
");
        }

        foreach (var q in feature.Queries ?? [])
        {
            var queryName = q.Name!.EndsWith("Query", StringComparison.Ordinal) ? q.Name : q.Name + "Query";
            var dtoName = queryName.Replace("Query", "Dto");
            WriteIfMissing(Path.Combine(queriesDir, queryName + ".cs"), $"""
using MediatR;

namespace {ns}.Features.{feature.Name}.Queries;

public sealed record {queryName}({FormatProperties(q.Properties)}) : IRequest<{dtoName}?>;
public sealed record {dtoName}(string Id);
""");
            WriteIfMissing(Path.Combine(handlersDir, queryName + "Handler.cs"), $@"using MediatR;

namespace {ns}.Features.{feature.Name}.Handlers;

public sealed class {queryName}Handler : IRequestHandler<Queries.{queryName}, Queries.{dtoName}?>
{{
    public Task<Queries.{dtoName}?> Handle(Queries.{queryName} request, CancellationToken ct)
        => Task.FromResult<Queries.{dtoName}?>(new Queries.{dtoName}(request.Id ?? string.Empty));
}}
");
        }
    }


    private static string FormatProperties(List<PropertySpec>? props)
    {
        if (props is null || props.Count == 0) return "string? Id = null";
        return string.Join(", ", props.Select(p => $"{p.Type} {p.Name}"));
    }

    private static void WriteIfMissing(string path, string content)
    {
        if (File.Exists(path)) return;
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, content);
    }
}
