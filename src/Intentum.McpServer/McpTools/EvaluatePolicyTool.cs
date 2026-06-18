using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Policy;

namespace Intentum.McpServer.McpTools;

public sealed class EvaluatePolicyTool
{
    public record RuleInput(string Name, string Decision);
    public record EvaluateRequest(string IntentName, double Score, string Level, IReadOnlyList<RuleInput> Rules);
    public record EvaluateResponse(string Decision);

    public EvaluateResponse Execute(EvaluateRequest request)
    {
        var intent = new Intent(request.IntentName, [],
            new IntentConfidence(request.Score, request.Level));

        var policy = new IntentPolicy();
        foreach (var rule in request.Rules)
        {
            if (Enum.TryParse<PolicyDecision>(rule.Decision, out var decision))
            {
                policy.AddRule(new PolicyRule(rule.Name, _ => true, decision));
            }
        }

        var result = IntentPolicyEngine.Evaluate(intent, policy);
        return new EvaluateResponse(result.ToString());
    }
}
