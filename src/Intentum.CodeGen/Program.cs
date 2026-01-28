using System.CommandLine;
using Intentum.CodeGen;

var scaffold = new Command("scaffold", "Scaffold Intentum + CQRS project (new or into existing folder).");
var outputOption = new Option<DirectoryInfo>("--output", "Target directory") { Arity = ArgumentArity.ZeroOrOne };
outputOption.AddAlias("-o");
outputOption.SetDefaultValueFactory(() => new DirectoryInfo("."));
scaffold.AddOption(outputOption);
scaffold.SetHandler((DirectoryInfo o) => ScaffoldRunner.Run(o), outputOption);

var generate = new Command("generate", "Generate CQRS feature code from test assembly or YAML/JSON spec.");
var specOption = new Option<FileInfo?>("--spec", "Path to YAML or JSON spec file");
specOption.AddAlias("-s");
var assemblyOption = new Option<FileInfo?>("--assembly", "Path to test assembly (e.g. MyApp.Tests.dll)");
assemblyOption.AddAlias("-a");
var genOutputOption = new Option<DirectoryInfo>("--output", "Target project directory") { Arity = ArgumentArity.ZeroOrOne };
genOutputOption.AddAlias("-o");
genOutputOption.SetDefaultValueFactory(() => new DirectoryInfo("."));
generate.AddOption(specOption);
generate.AddOption(assemblyOption);
generate.AddOption(genOutputOption);
generate.SetHandler(async (FileInfo? s, FileInfo? a, DirectoryInfo o) => await SpecCodeGenerator.Run(s, a, o), specOption, assemblyOption, genOutputOption);

var root = new RootCommand("Intentum CodeGen: scaffold CQRS + Intentum projects and generate Features from spec or tests.");
root.AddCommand(scaffold);
root.AddCommand(generate);

return await root.InvokeAsync(args);
