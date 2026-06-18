using Intentum.Core.Intents;
using Intentum.Runtime.Engine;
using Intentum.Runtime.Localization;
using Intentum.Runtime.Policy;
using Intentum.Runtime.RateLimiting;

namespace Intentum.Runtime;

public static class RuntimeExtensions
{
    [Obsolete("Use DecideWithRateLimitAsync for async scenarios. This method will be removed in v2.0.")]
    public static PolicyDecision Decide(
        this Intent intent,
        IntentPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(policy);
        return IntentPolicyEngine.Evaluate(intent, policy);
    }

    /// <summary>
    /// Decides using context-aware policy (intent + system load, region, etc.).
    /// </summary>
    public static PolicyDecision Decide(
        this Intent intent,
        PolicyContext context,
        ContextAwareIntentPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(policy);
        return ContextAwarePolicyEngine.Evaluate(intent, context, policy);
    }

    /// <summary>
    /// Decides using an A/B policy variant set (selector picks which policy applies to this intent).
    /// </summary>
    public static PolicyDecision Decide(
        this Intent intent,
        PolicyVariantSet variantSet)
    {
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(variantSet);
        return variantSet.Decide(intent);
    }

    /// <summary>
    /// Decides policy and, when decision is RateLimit, checks the rate limiter.
    /// Returns the policy decision; use <paramref name="rateLimitResult"/> when decision is RateLimit.
    /// </summary>
    public static PolicyDecision DecideWithRateLimit(
        this Intent intent,
        IntentPolicy policy,
        IRateLimiter rateLimiter,
        RateLimitOptions options,
        out RateLimitResult? rateLimitResult,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(rateLimiter);
        ArgumentNullException.ThrowIfNull(options);

        var decision = IntentPolicyEngine.Evaluate(intent, policy);
        rateLimitResult = null;
        if (decision != PolicyDecision.RateLimit)
            return decision;
        var result = rateLimiter.TryAcquireAsync(options.Key, options.Limit, options.Window, cancellationToken).AsTask().GetAwaiter().GetResult();
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
        RateLimitOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(rateLimiter);
        ArgumentNullException.ThrowIfNull(options);

        var decision = IntentPolicyEngine.Evaluate(intent, policy);
        if (decision != PolicyDecision.RateLimit)
            return (decision, null);
        var result = await rateLimiter.TryAcquireAsync(options.Key, options.Limit, options.Window, cancellationToken);
        return (decision, result);
    }

    public static string ToLocalizedString(
        this PolicyDecision decision,
        IIntentumLocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(localizer);

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
