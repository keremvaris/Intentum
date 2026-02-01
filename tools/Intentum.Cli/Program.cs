using System.CommandLine;
using System.Diagnostics;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Models;

namespace Intentum.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var root = new RootCommand("Intentum CLI: create project, validate model/policy, run infer.");
        root.AddCommand(NewCommand());
        root.AddCommand(ValidateCommand());
        root.AddCommand(InferCommand());
        return await root.InvokeAsync(args);
    }

    private static Command NewCommand()
    {
        var name = new Option<string>("--name", "Project name") { IsRequired = true };
        var output = new Option<DirectoryInfo>("--output", () => new DirectoryInfo("."), "Output directory");
        var template = new Option<string>("--template", () => "intentum-webapi", "Template: intentum-webapi, intentum-backgroundservice, intentum-function, intentum-cqrs");
        var cmd = new Command("new", "Create a new project from an Intentum template.");
        cmd.AddOption(name);
        cmd.AddOption(output);
        cmd.AddOption(template);
        cmd.SetHandler((nameVal, outputVal, templateVal) =>
        {
            var dir = outputVal.FullName;
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                ArgumentList = { "new", templateVal, "-n", nameVal, "-o", dir },
                UseShellExecute = false
            });
            process?.WaitForExit();
            return Task.FromResult(process?.ExitCode ?? 1);
        }, name, output, template);
        return cmd;
    }

    private static Command ValidateCommand()
    {
        var modelPath = new Option<FileInfo?>("--model", "Path to model file (e.g. ONNX)");
        var policyPath = new Option<FileInfo?>("--policy", "Path to policy JSON file");
        var cmd = new Command("validate", "Validate model or policy file.");
        cmd.AddOption(modelPath);
        cmd.AddOption(policyPath);
        cmd.SetHandler((model, policy) =>
        {
            if (model != null)
            {
                if (!model.Exists) { Console.Error.WriteLine($"Model file not found: {model.FullName}"); return Task.FromResult(1); }
                Console.WriteLine($"Model file OK: {model.FullName}");
            }
            if (policy != null)
            {
                if (!policy.Exists) { Console.Error.WriteLine($"Policy file not found: {policy.FullName}"); return Task.FromResult(1); }
                Console.WriteLine($"Policy file OK: {policy.FullName}");
            }
            if (model == null && policy == null) { Console.Error.WriteLine("Specify --model or --policy"); return Task.FromResult(1); }
            return Task.FromResult(0);
        }, modelPath, policyPath);
        return cmd;
    }

    private static Command InferCommand()
    {
        var events = new Option<string[]>("--events", "Events as actor:action (e.g. user:login user:submit)");
        var cmd = new Command("infer", "Run intent inference with rule-based model (demo).");
        cmd.AddOption(events);
        cmd.SetHandler((eventsVal) =>
        {
            var space = new BehaviorSpace();
            var list = eventsVal;
            foreach (var e in list)
            {
                var parts = e.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                    space.Observe(parts[0], parts[1]);
            }
            var rules = new List<Func<BehaviorSpace, RuleMatch?>>
            {
                s => s.Events.Count(ev => ev.Action == "login.failed") >= 2
                    ? new RuleMatch("Suspicious", 0.75, "login.failed>=2")
                    : null,
                s => s.Events.Any(ev => ev.Action == "submit")
                    ? new RuleMatch("Submit", 0.9, "submit")
                    : null
            };
            var model = new RuleBasedIntentModel(rules);
            var intent = model.Infer(space);
            Console.WriteLine($"Intent: {intent.Name} | Confidence: {intent.Confidence.Level} ({intent.Confidence.Score:F2}) | Reasoning: {intent.Reasoning ?? "-"}");
            return Task.FromResult(0);
        }, events);
        return cmd;
    }
}
