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
        command.SetHandler((string nameOption) =>
        {
            var template = $@"using Intentum.Core.Behavior;
using Intentum.Core.Contracts;
using Intentum.Core.Intents;

namespace MyProject.Models;

public class {nameOption}Model : IIntentModel
{{
    public Intent Infer(BehaviorSpace behaviorSpace, BehaviorVector? precomputedVector = null)
    {{
        // TODO: Implement intent inference
        throw new NotImplementedException();
    }}
}}";
            File.WriteAllText($"{nameOption}Model.cs", template);
            Console.WriteLine($"Created {nameOption}Model.cs");
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
        command.SetHandler((string nameOption) =>
        {
            var template = $@"using Intentum.Runtime.Policy;
using Intentum.Core.Intents;

namespace MyProject.Policies;

public static class {nameOption}Policy
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
            File.WriteAllText($"{nameOption}Policy.cs", template);
            Console.WriteLine($"Created {nameOption}Policy.cs");
        }, nameOption);
        return command;
    }
}
