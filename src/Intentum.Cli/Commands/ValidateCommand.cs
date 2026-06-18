using System.CommandLine;

namespace Intentum.Cli.Commands;

public static class ValidateCommand
{
    public static Option<string> PathOption { get; } =
        new Option<string>("--path", "Path to validate") { IsRequired = true };

    public static async Task Handler(string path)
    {
        if (!Directory.Exists(path))
        {
            Console.Error.WriteLine($"Path not found: {path}");
            return;
        }

        var csFiles = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
        Console.WriteLine($"Found {csFiles.Length} C# files");

        foreach (var file in csFiles)
        {
            var content = await File.ReadAllTextAsync(file);
            if (content.Contains("IIntentModel") && !content.Contains("Infer"))
            {
                Console.WriteLine($"WARNING: {file} implements IIntentModel but missing Infer method");
            }
        }

        Console.WriteLine("Validation complete");
    }
}
