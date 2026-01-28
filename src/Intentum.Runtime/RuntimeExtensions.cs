using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Localization;
using Intentum.Runtime.Policy;

namespace Intentum.Runtime;

public static class RuntimeExtensions
{
    public static PolicyDecision Decide(
        this Intent intent,
        IntentPolicy policy)
    {
        return IntentPolicyEngine.Evaluate(intent, policy);
    }

    public static string ToLocalizedString(
        this PolicyDecision decision,
        IIntentumLocalizer localizer)
    {
        var key = decision switch
        {
            PolicyDecision.Allow => LocalizationKeys.DecisionAllow,
            PolicyDecision.Observe => LocalizationKeys.DecisionObserve,
            PolicyDecision.Warn => LocalizationKeys.DecisionWarn,
            PolicyDecision.Block => LocalizationKeys.DecisionBlock,
            _ => decision.ToString()
        };

        return localizer.Get(key);
    }
}
