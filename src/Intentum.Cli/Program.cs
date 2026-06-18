using System.CommandLine;
using Intentum.Cli.Commands;

var rootCommand = new RootCommand("Intentum CLI - Intent-driven development tools");

var scaffoldCommand = new Command("scaffold", "Scaffold Intentum components");
scaffoldCommand.AddCommand(ScaffoldCommand.CreateModelCommand());
scaffoldCommand.AddCommand(ScaffoldCommand.CreatePolicyCommand());

var validateCommand = new Command("validate", "Validate configuration");
validateCommand.AddOption(ValidateCommand.PathOption);
validateCommand.SetHandler(ValidateCommand.Handler, ValidateCommand.PathOption);

var versionCommand = new Command("version", "Show version");
versionCommand.SetHandler(() => Console.WriteLine("Intentum CLI v1.1.8"));

rootCommand.AddCommand(scaffoldCommand);
rootCommand.AddCommand(validateCommand);
rootCommand.AddCommand(versionCommand);

return await rootCommand.InvokeAsync(args);
