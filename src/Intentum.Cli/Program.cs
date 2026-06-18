using System.CommandLine;
using Intentum.Cli.Commands;

var rootCommand = new RootCommand("Intentum CLI - Intent-driven development tools");

var scaffoldCommand = new Command("scaffold", "Scaffold Intentum components");
scaffoldCommand.AddCommand(ScaffoldCommand.CreateModelCommand());
scaffoldCommand.AddCommand(ScaffoldCommand.CreatePolicyCommand());

var validateCommand = new Command("validate", "Validate configuration");
validateCommand.AddOption(ValidateCommand.PathOption);
validateCommand.SetHandler(async (string path) =>
{
    var exitCode = await ValidateCommand.Handler(path);
    Environment.ExitCode = exitCode;
}, ValidateCommand.PathOption);

var versionCommand = new Command("version", "Show version");
var version = typeof(Program).Assembly.GetName().Version;
var versionString = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.1.8";
versionCommand.SetHandler(() => Console.WriteLine($"Intentum CLI v{versionString}"));

rootCommand.AddCommand(scaffoldCommand);
rootCommand.AddCommand(validateCommand);
rootCommand.AddCommand(versionCommand);

return await rootCommand.InvokeAsync(args);
