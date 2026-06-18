using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Core.SupplyChain;

namespace Intentum.Tests;

public sealed class SupplyChainRulesTests
{
    [Fact]
    public void InventoryShortage_WithTwoSignals_ReturnsMatch()
    {
        var rule = SupplyChainRules.InventoryShortage();
        var space = new BehaviorSpace()
            .Observe("user", "stock.low")
            .Observe("user", "demand.spike");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("InventoryShortage", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void InventoryShortage_WithOneSignal_ReturnsNull()
    {
        var rule = SupplyChainRules.InventoryShortage();
        var space = new BehaviorSpace()
            .Observe("user", "stock.low");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void InventoryShortage_WithFourSignals_ReturnsMatch()
    {
        var rule = SupplyChainRules.InventoryShortage();
        var space = new BehaviorSpace()
            .Observe("user", "stock.low")
            .Observe("user", "demand.spike")
            .Observe("user", "reorder.failed")
            .Observe("user", "backorder.created");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("InventoryShortage", match.Name);
    }

    [Fact]
    public void SupplierRisk_WithTwoSignals_ReturnsMatch()
    {
        var rule = SupplyChainRules.SupplierRisk();
        var space = new BehaviorSpace()
            .Observe("user", "delivery.delayed")
            .Observe("user", "quality.issue");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("SupplierRisk", match.Name);
        Assert.Equal(0.8, match.Score);
    }

    [Fact]
    public void SupplierRisk_WithOneSignal_ReturnsNull()
    {
        var rule = SupplyChainRules.SupplierRisk();
        var space = new BehaviorSpace()
            .Observe("user", "compliance.flag");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void LogisticsDisruption_WithTwoSignals_ReturnsMatch()
    {
        var rule = SupplyChainRules.LogisticsDisruption();
        var space = new BehaviorSpace()
            .Observe("user", "route.changed")
            .Observe("user", "carrier.service.issue");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("LogisticsDisruption", match.Name);
        Assert.Equal(0.9, match.Score);
    }

    [Fact]
    public void LogisticsDisruption_WithOneSignal_ReturnsNull()
    {
        var rule = SupplyChainRules.LogisticsDisruption();
        var space = new BehaviorSpace()
            .Observe("user", "customs.flag");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void DemandForecastAnomaly_WithTwoSignals_ReturnsMatch()
    {
        var rule = SupplyChainRules.DemandForecastAnomaly();
        var space = new BehaviorSpace()
            .Observe("user", "order.pattern.unexpected")
            .Observe("user", "demand.seasonal.deviation");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("DemandForecastAnomaly", match.Name);
        Assert.Equal(0.75, match.Score);
    }

    [Fact]
    public void DemandForecastAnomaly_WithOneSignal_ReturnsNull()
    {
        var rule = SupplyChainRules.DemandForecastAnomaly();
        var space = new BehaviorSpace()
            .Observe("user", "forecast.accuracy.declining");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void WarehouseCapacity_WithTwoSignals_ReturnsMatch()
    {
        var rule = SupplyChainRules.WarehouseCapacity();
        var space = new BehaviorSpace()
            .Observe("user", "warehouse.utilization.spike")
            .Observe("user", "warehouse.throughput.drop");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("WarehouseCapacity", match.Name);
        Assert.Equal(0.8, match.Score);
    }

    [Fact]
    public void WarehouseCapacity_WithOneSignal_ReturnsNull()
    {
        var rule = SupplyChainRules.WarehouseCapacity();
        var space = new BehaviorSpace()
            .Observe("user", "warehouse.bottleneck");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void OrderFulfillmentDelay_WithTwoSignals_ReturnsMatch()
    {
        var rule = SupplyChainRules.OrderFulfillmentDelay();
        var space = new BehaviorSpace()
            .Observe("user", "order.processing.delayed")
            .Observe("user", "order.pick.pack.issue");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("OrderFulfillmentDelay", match.Name);
        Assert.Equal(0.85, match.Score);
    }

    [Fact]
    public void OrderFulfillmentDelay_WithOneSignal_ReturnsNull()
    {
        var rule = SupplyChainRules.OrderFulfillmentDelay();
        var space = new BehaviorSpace()
            .Observe("user", "order.shipping.delayed");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void SupplierDependencyRisk_WithOneSignal_ReturnsMatch()
    {
        var rule = SupplyChainRules.SupplierDependencyRisk();
        var space = new BehaviorSpace()
            .Observe("user", "supplier.single_source");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("SupplierDependencyRisk", match.Name);
        Assert.Equal(0.8, match.Score);
    }

    [Fact]
    public void SupplierDependencyRisk_WithNoSignals_ReturnsNull()
    {
        var rule = SupplyChainRules.SupplierDependencyRisk();
        var space = new BehaviorSpace()
            .Observe("user", "some.other.event");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void ReturnsAnomaly_WithTwoSignals_ReturnsMatch()
    {
        var rule = SupplyChainRules.ReturnsAnomaly();
        var space = new BehaviorSpace()
            .Observe("user", "return.rate.unusual")
            .Observe("user", "return.pattern.changed");

        var match = rule(space);

        Assert.NotNull(match);
        Assert.Equal("ReturnsAnomaly", match.Name);
        Assert.Equal(0.8, match.Score);
    }

    [Fact]
    public void ReturnsAnomaly_WithOneSignal_ReturnsNull()
    {
        var rule = SupplyChainRules.ReturnsAnomaly();
        var space = new BehaviorSpace()
            .Observe("user", "return.fraud.suspected");

        var match = rule(space);

        Assert.Null(match);
    }

    [Fact]
    public void AllRules_ReturnsEightRules()
    {
        var rules = SupplyChainRules.AllRules();

        Assert.Equal(8, rules.Count);
    }

    [Fact]
    public void AllRules_FirstRuleIsLogisticsDisruption()
    {
        var rules = SupplyChainRules.AllRules();
        var space = new BehaviorSpace()
            .Observe("user", "route.changed")
            .Observe("user", "carrier.service.issue");

        var match = rules[0](space);

        Assert.NotNull(match);
        Assert.Equal("LogisticsDisruption", match.Name);
    }

    [Fact]
    public void AllRules_AllRulesReturnMatchForTheirSignals()
    {
        var rules = SupplyChainRules.AllRules();
        var space = new BehaviorSpace()
            .Observe("user", "stock.low")
            .Observe("user", "demand.spike")
            .Observe("user", "delivery.delayed")
            .Observe("user", "quality.issue")
            .Observe("user", "route.changed")
            .Observe("user", "customs.flag")
            .Observe("user", "order.pattern.unexpected")
            .Observe("user", "demand.seasonal.deviation")
            .Observe("user", "warehouse.utilization.spike")
            .Observe("user", "warehouse.throughput.drop")
            .Observe("user", "order.processing.delayed")
            .Observe("user", "order.pick.pack.issue")
            .Observe("user", "supplier.single_source")
            .Observe("user", "return.rate.unusual")
            .Observe("user", "return.pattern.changed");

        foreach (var rule in rules)
        {
            var match = rule(space);
            Assert.NotNull(match);
        }
    }
}
