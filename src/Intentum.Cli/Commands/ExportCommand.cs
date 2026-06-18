using System.CommandLine;

namespace Intentum.Cli.Commands;

public static class ExportCommand
{
    public static Command Create()
    {
        var formatOption = new Option<string>("--format", () => "yaml", "Export format (yaml or json)");
        var outputOption = new Option<string>("--output", "Output file path");

        var command = new Command("export", "Export OpenAPI specification")
        {
            formatOption,
            outputOption
        };
        command.SetHandler((string format, string output) =>
        {
            var specDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "docs", "openapi");
            var specPath = Path.Combine(specDir, "intentum.yaml");
            
            if (!File.Exists(specPath))
            {
                // Try relative to current directory
                specPath = Path.Combine(Environment.CurrentDirectory, "docs", "openapi", "intentum.yaml");
            }
            
            if (!File.Exists(specPath))
            {
                Console.Error.WriteLine("OpenAPI spec not found. Run from project root directory.");
                return;
            }

            var target = output ?? $"intentum.{format}";
            File.Copy(specPath, target, true);
            Console.WriteLine($"Exported OpenAPI spec to {target}");
        }, formatOption, outputOption);
        return command;
    }
}
