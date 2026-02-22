using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.Commerce;

namespace Intentum.Tests;

/// <summary>
/// Tests for CommerceRules: PurchaseIntent, CartAbandonment, BrowsingIntent, SupportIntent.
/// </summary>
public sealed class CommerceRulesTests
{
    [Fact]
    public void PurchaseIntent_WithCartAndCheckout_ReturnsMatch()
    {
        var rule = CommerceRules.PurchaseIntent();
        var space = new BehaviorSpace()
            .Observe("user", "product.view")
            .Observe("user", "cart.add")
            .Observe("user", "checkout");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("PurchaseIntent", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void PurchaseIntent_WithMultipleCartAdds_ReturnsMatch()
    {
        var rule = CommerceRules.PurchaseIntent();
        var space = new BehaviorSpace()
            .Observe("user", "AddToCart")
            .Observe("user", "cart.add");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("PurchaseIntent", match.Name);
    }

    [Fact]
    public void PurchaseIntent_WithNoCart_ReturnsNull()
    {
        var rule = CommerceRules.PurchaseIntent();
        var space = new BehaviorSpace().Observe("user", "product.view");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void CartAbandonment_WithCartAddAndNoPurchase_ReturnsMatch()
    {
        var rule = CommerceRules.CartAbandonment();
        var space = new BehaviorSpace()
            .Observe("user", "cart.add")
            .Observe("user", "navigate.away");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("CartAbandonment", match.Name);
    }

    [Fact]
    public void CartAbandonment_WithPurchase_ReturnsNull()
    {
        var rule = CommerceRules.CartAbandonment();
        var space = new BehaviorSpace()
            .Observe("user", "cart.add")
            .Observe("user", "purchase");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void BrowsingIntent_WithManyViewsAndNoCart_ReturnsMatch()
    {
        var rule = CommerceRules.BrowsingIntent(minViews: 5);
        var space = new BehaviorSpace();
        for (var i = 0; i < 6; i++)
            space.Observe("user", "product.view");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("BrowsingIntent", match.Name);
    }

    [Fact]
    public void BrowsingIntent_WithCartActivity_ReturnsNull()
    {
        var rule = CommerceRules.BrowsingIntent(minViews: 3);
        var space = new BehaviorSpace()
            .Observe("user", "view")
            .Observe("user", "browse")
            .Observe("user", "search")
            .Observe("user", "cart.add");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void SupportIntent_WithMultipleSupportActions_ReturnsMatch()
    {
        var rule = CommerceRules.SupportIntent();
        var space = new BehaviorSpace()
            .Observe("user", "support")
            .Observe("user", "help");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("SupportIntent", match.Name);
    }

    [Fact]
    public void AllRules_ReturnsFourRules()
    {
        var rules = CommerceRules.AllRules();

        Assert.Equal(4, rules.Count);
    }
}
