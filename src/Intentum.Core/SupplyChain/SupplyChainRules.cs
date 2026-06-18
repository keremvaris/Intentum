using Intentum.Core.Behavior;
using Intentum.Core.Models;

namespace Intentum.Core.SupplyChain;

public static class SupplyChainRules
{
    public static Func<BehaviorSpace, RuleMatch?> InventoryShortage(
        double confidence = 0.85) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("stock.low", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("demand.spike", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("reorder.failed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("backorder.created", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("InventoryShortage", confidence,
                $"Inventory shortage signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> SupplierRisk(
        double confidence = 0.8) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("delivery.delayed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("quality.issue", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("communication.gap", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("compliance.flag", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("SupplierRisk", confidence,
                $"Supplier risk signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> LogisticsDisruption(
        double confidence = 0.9) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("route.changed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("carrier.service.issue", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("customs.flag", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("transit.delayed", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("LogisticsDisruption", confidence,
                $"Logistics disruption signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> DemandForecastAnomaly(
        double confidence = 0.75) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("order.pattern.unexpected", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("demand.seasonal.deviation", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("forecast.accuracy.declining", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("DemandForecastAnomaly", confidence,
                $"Demand forecast anomaly signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> WarehouseCapacity(
        double confidence = 0.8) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("warehouse.utilization.spike", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("warehouse.throughput.drop", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("warehouse.capacity.exceeded", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("warehouse.bottleneck", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("WarehouseCapacity", confidence,
                $"Warehouse capacity signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> OrderFulfillmentDelay(
        double confidence = 0.85) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("order.processing.delayed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("order.pick.pack.issue", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("order.shipping.delayed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("order.inventory.short", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("OrderFulfillmentDelay", confidence,
                $"Order fulfillment delay signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> SupplierDependencyRisk(
        double confidence = 0.8) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("supplier.single_source", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("supplier.geopolitical.flag", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("supplier.financial.instability", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("supplier.capacity.limited", StringComparison.OrdinalIgnoreCase));

        if (signals >= 1)
            return new RuleMatch("SupplierDependencyRisk", confidence,
                $"Supplier dependency risk signals: {signals}");
        return null;
    };

    public static Func<BehaviorSpace, RuleMatch?> ReturnsAnomaly(
        double confidence = 0.8) => space =>
    {
        var signals = space.Events.Count(e =>
            e.Action.Contains("return.rate.unusual", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("return.pattern.changed", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("return.fraud.suspected", StringComparison.OrdinalIgnoreCase) ||
            e.Action.Contains("return.volume.spike", StringComparison.OrdinalIgnoreCase));

        if (signals >= 2)
            return new RuleMatch("ReturnsAnomaly", confidence,
                $"Returns anomaly signals: {signals}");
        return null;
    };

    public static IReadOnlyList<Func<BehaviorSpace, RuleMatch?>> AllRules() =>
    [
        LogisticsDisruption(),
        InventoryShortage(),
        SupplierRisk(),
        DemandForecastAnomaly(),
        WarehouseCapacity(),
        OrderFulfillmentDelay(),
        SupplierDependencyRisk(),
        ReturnsAnomaly()
    ];
}
