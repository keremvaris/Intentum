using Intentum.Core.Behavior;
using Intentum.Core.Models;

namespace Intentum.Core.Commerce;

/// <summary>
/// Pre-built e-commerce intent rules for customer behavior classification.
/// </summary>
public static class CommerceRules
{
    /// <summary>
    /// Detects purchase intent: product views, cart adds, checkout page visits.
    /// </summary>
    public static Func<BehaviorSpace, RuleMatch?> PurchaseIntent(
        double confidence = 0.85) => space =>
    {
        var productViews = space.Events.Count(e =>
            e.Action.Contains("product.view", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("ViewProduct", StringComparison.OrdinalIgnoreCase));
        var cartAdds = space.Events.Count(e =>
            e.Action.Contains("cart.add", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("AddToCart", StringComparison.OrdinalIgnoreCase));
        var checkoutVisit = space.Events.Any(e =>
            e.Action.Contains("checkout", StringComparison.OrdinalIgnoreCase));

        if (cartAdds >= 1 && checkoutVisit)
            return new RuleMatch("PurchaseIntent", confidence,
                $"Views: {productViews}, cart adds: {cartAdds}, checkout: true");
        if (cartAdds >= 2)
            return new RuleMatch("PurchaseIntent", confidence * 0.7,
                $"Multiple cart adds: {cartAdds}");
        return null;
    };

    /// <summary>
    /// Detects cart abandonment: cart activity followed by browsing away or leaving.
    /// </summary>
    public static Func<BehaviorSpace, RuleMatch?> CartAbandonment(
        double confidence = 0.75) => space =>
    {
        var cartAdds = space.Events.Count(e =>
            e.Action.Contains("cart.add", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("AddToCart", StringComparison.OrdinalIgnoreCase));
        var cartRemoves = space.Events.Count(e =>
            e.Action.Contains("cart.remove", StringComparison.OrdinalIgnoreCase));
        var hasCheckout = space.Events.Any(e =>
            e.Action.Contains("checkout", StringComparison.OrdinalIgnoreCase));
        var hasPurchase = space.Events.Any(e =>
            e.Action.Contains("purchase", StringComparison.OrdinalIgnoreCase));
        var hasBrowseAway = space.Events.Any(e =>
            e.Action.Contains("navigate.away", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("session.end", StringComparison.OrdinalIgnoreCase));

        if (cartAdds >= 1 && !hasPurchase && (cartRemoves >= 1 || hasBrowseAway || !hasCheckout))
            return new RuleMatch("CartAbandonment", confidence,
                $"Cart adds: {cartAdds}, removes: {cartRemoves}, no purchase, browse away: {hasBrowseAway}");
        return null;
    };

    /// <summary>
    /// Detects browsing/research intent: extensive product views without cart activity.
    /// </summary>
    public static Func<BehaviorSpace, RuleMatch?> BrowsingIntent(
        int minViews = 5,
        double confidence = 0.7) => space =>
    {
        var views = space.Events.Count(e =>
            e.Action.Contains("view", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("browse", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("search", StringComparison.OrdinalIgnoreCase));
        var cartAdds = space.Events.Count(e =>
            e.Action.Contains("cart", StringComparison.OrdinalIgnoreCase));

        if (views >= minViews && cartAdds == 0)
            return new RuleMatch("BrowsingIntent", confidence,
                $"Views/searches: {views}, no cart activity");
        return null;
    };

    /// <summary>
    /// Detects support/help-seeking intent.
    /// </summary>
    public static Func<BehaviorSpace, RuleMatch?> SupportIntent(
        double confidence = 0.8) => space =>
    {
        var supportActions = space.Events.Count(e =>
            e.Action.Contains("support", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("help", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("faq", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("contact", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("return", StringComparison.OrdinalIgnoreCase));

        if (supportActions >= 2)
            return new RuleMatch("SupportIntent", confidence,
                $"Support-related actions: {supportActions}");
        return null;
    };

    /// <summary>
    /// Returns all standard e-commerce rules in recommended evaluation order.
    /// </summary>
    public static IReadOnlyList<Func<BehaviorSpace, RuleMatch?>> AllRules() =>
    [
        PurchaseIntent(),
        CartAbandonment(),
        SupportIntent(),
        BrowsingIntent()
    ];
}
