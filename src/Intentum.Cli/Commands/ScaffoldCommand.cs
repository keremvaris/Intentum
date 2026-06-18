using System.CommandLine;

namespace Intentum.Cli.Commands;

public static class ScaffoldCommand
{
    public static Command CreateModelCommand()
    {
        var nameOption = new Option<string>("--name", "Model name") { IsRequired = true };
        var command = new Command("model", "Create IIntentModel implementation")
        {
            nameOption
        };
        command.SetHandler((string nameValue) =>
        {
            try
            {
                var template = $@"using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

#nullable enable
namespace MyNamespace;

public class {nameValue}Model : IIntentModel
{{
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {{
        // TODO: Implement intent inference
        throw new NotImplementedException();
    }}
}}";
                File.WriteAllText($"{nameValue}Model.cs", template);
                Console.WriteLine($"Created {nameValue}Model.cs");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating model: {ex.Message}");
            }
        }, nameOption);
        return command;
    }

    public static Command CreatePolicyCommand()
    {
        var nameOption = new Option<string>("--name", "Policy name") { IsRequired = true };
        var command = new Command("policy", "Create IntentPolicy with rules")
        {
            nameOption
        };
        command.SetHandler((string nameValue) =>
        {
            try
            {
                var template = $@"using Intentum.Runtime.Policy;
using Intentum.Core.Intents;

#nullable enable
namespace MyNamespace;

public static class {nameValue}Policy
{{
    public static IntentPolicy Create()
    {{
        return new IntentPolicy()
            .AddRule(new PolicyRule(
                ""HighConfidence"",
                i => i.Confidence.Level == ""High"",
                PolicyDecision.Allow));
    }}
}}";
                File.WriteAllText($"{nameValue}Policy.cs", template);
                Console.WriteLine($"Created {nameValue}Policy.cs");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating policy: {ex.Message}");
            }
        }, nameOption);
        return command;
    }
}
