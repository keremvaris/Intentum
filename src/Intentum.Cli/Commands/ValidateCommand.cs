using System.CommandLine;
using System.Text.RegularExpressions;

namespace Intentum.Cli.Commands;

public static class ValidateCommand
{
    public static Option<string> PathOption { get; } =
        new Option<string>("--path", "Path to validate") { IsRequired = true };

    public static async Task<int> Handler(string path)
    {
        if (!Directory.Exists(path))
        {
            Console.Error.WriteLine($"Path not found: {path}");
            return 1;
        }

        var csFiles = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
        Console.WriteLine($"Found {csFiles.Length} C# files");

        if (csFiles.Length == 0)
        {
            Console.WriteLine("No C# files found. Nothing to validate.");
            return 0;
        }

        var warningCount = 0;
        var intentModelRegex = new Regex(@":\s*IIntentModel\b", RegexOptions.Compiled);

        foreach (var file in csFiles)
        {
            var content = await File.ReadAllTextAsync(file);
            if (intentModelRegex.IsMatch(content) && !content.Contains("Infer("))
            {
                Console.WriteLine($"WARNING: {file} implements IIntentModel but missing Infer method");
                warningCount++;
            }
        }

        if (warningCount > 0)
        {
            Console.WriteLine($"Validation complete: {warningCount} warning(s) found");
            return 1;
        }

        Console.WriteLine("No issues found. Validation complete.");
        return 0;
    }
}
