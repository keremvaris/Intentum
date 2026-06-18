using System.CommandLine;
using System.Text.Json;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;

namespace Intentum.Cli.Commands;

public static class TestInferCommand
{
    public static Option<string> DataOption { get; } =
        new Option<string>("--data", "JSON file with events") { IsRequired = true };

    public static Command Create()
    {
        var command = new Command("test-infer", "Test inference with sample data")
        {
            DataOption
        };
        command.SetHandler((string dataOption) =>
        {
            if (!File.Exists(dataOption))
            {
                Console.Error.WriteLine($"File not found: {dataOption}");
                return;
            }

            var json = File.ReadAllText(dataOption);
            var events = JsonSerializer.Deserialize<List<BehaviorEventDto>>(json);

            if (events == null || events.Count == 0)
            {
                Console.Error.WriteLine("Invalid JSON format or empty events array");
                return;
            }

            var space = new BehaviorSpace();
            foreach (var evt in events)
            {
                space.Observe(new BehaviorEvent(evt.Actor, evt.Action, DateTimeOffset.UtcNow));
            }

            Console.WriteLine($"Loaded {space.Events.Count} events");
            Console.WriteLine($"Vector dimensions: {space.ToVector().Dimensions.Count}");
        }, DataOption);
        return command;
    }

    private record BehaviorEventDto(string Actor, string Action);
}
