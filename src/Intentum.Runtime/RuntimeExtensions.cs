using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Localization;
using Intentum.Runtime.Policy;
using Intentum.Runtime.RateLimiting;

namespace Intentum.Runtime;

public static class RuntimeExtensions
{
    public static PolicyDecision Decide(
        this Intent intent,
        IntentPolicy policy)
    {
        return IntentPolicyEngine.Evaluate(intent, policy);
    }

    /// <summary>
    /// Decides policy and, when decision is RateLimit, checks the rate limiter.
    /// Returns the policy decision; use <paramref name="rateLimitResult"/> when decision is RateLimit.
    /// </summary>
    public static PolicyDecision DecideWithRateLimit(
        this Intent intent,
        IntentPolicy policy,
        IRateLimiter rateLimiter,
        string rateLimitKey,
        int limit,
        TimeSpan window,
        out RateLimitResult? rateLimitResult,
        CancellationToken cancellationToken = default)
    {
        var decision = IntentPolicyEngine.Evaluate(intent, policy);
        rateLimitResult = null;
        if (decision != PolicyDecision.RateLimit)
            return decision;
        var result = rateLimiter.TryAcquireAsync(rateLimitKey, limit, window, cancellationToken).AsTask().GetAwaiter().GetResult();
        rateLimitResult = result;
        return decision;
    }

    /// <summary>
    /// Async version: decides policy and, when decision is RateLimit, checks the rate limiter.
    /// </summary>
    public static async ValueTask<(PolicyDecision Decision, RateLimitResult? RateLimitResult)> DecideWithRateLimitAsync(
        this Intent intent,
        IntentPolicy policy,
        IRateLimiter rateLimiter,
        string rateLimitKey,
        int limit,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        var decision = IntentPolicyEngine.Evaluate(intent, policy);
        if (decision != PolicyDecision.RateLimit)
            return (decision, null);
        var result = await rateLimiter.TryAcquireAsync(rateLimitKey, limit, window, cancellationToken);
        return (decision, result);
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
            PolicyDecision.Escalate => LocalizationKeys.DecisionEscalate,
            PolicyDecision.RequireAuth => LocalizationKeys.DecisionRequireAuth,
            PolicyDecision.RateLimit => LocalizationKeys.DecisionRateLimit,
            _ => decision.ToString()
        };

        return localizer.Get(key);
    }
}
