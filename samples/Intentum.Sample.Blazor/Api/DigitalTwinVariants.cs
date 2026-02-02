using Intentum.Core.Behavior;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Digital Twin (Oracle of Operations) demo variants: metric events and recommendedScenario.
/// </summary>
public static class DigitalTwinVariants
{
    public const string VariantA = "A"; // Systemic bottleneck
    public const string VariantB = "B"; // Cost over speed
    public const string VariantC = "C"; // Stable
    public const string VariantD = "D"; // Single point of failure

    public static string GetExpectedIntent(string variant) => variant switch
    {
        VariantA => "ConvergingTowardSystemicBottleneckAndMissedSLAs",
        VariantB => "OptimizingForCostOverSpeed",
        VariantC => "StableWithinSLA",
        VariantD => "SinglePointOfFailure_Emerging",
        _ => "Unknown"
    };

    public static string GetRecommendedScenario(string variant) => variant switch
    {
        VariantA => "Picking_Robot_2 devre dışı bırakıldı; yedek robot rotaları yeniden hesaplandı. Konveyör yükü dağıtıldı.",
        VariantB => "SLA izleme artırıldı; maliyet/performans dengesi için uyarı eşiği ayarlandı.",
        VariantC => "Müdahale gerekmiyor; sistem baseline içinde.",
        VariantD => "Picking_Robot_2 bypass; yedek robot rotaları devreye alındı.",
        _ => ""
    };

    public static string GetLabel(string variant) => variant switch
    {
        VariantA => "Sistemik tıkanıklık",
        VariantB => "Maliyet odaklı",
        VariantC => "Stabil",
        VariantD => "Tek nokta arıza",
        _ => "Unknown"
    };

    public static BehaviorSpace BuildSpace(string variant, DateTimeOffset baseTime)
    {
        var space = new BehaviorSpace();
        space.SetMetadata("Variant", variant);
        var events = GetEvents(variant, baseTime);
        foreach (var (evt, _) in events)
            space.Observe(evt);
        return space;
    }

    public static IReadOnlyList<(BehaviorEvent Evt, string Summary)> GetEvents(string variant, DateTimeOffset baseTime)
    {
        return variant switch
        {
            VariantA => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("Picking_Robot_2", "ErrorRate_Report", baseTime, new Dictionary<string, object> { ["Value"] = 0.35, ["DeviationFromBaseline"] = 3.5, ["ErrorCategory"] = "Hardware" }), "Picking_Robot_2 ErrorRate +350%"),
                (new BehaviorEvent("system", "ExternalInput_Received", baseTime.AddMinutes(1), new Dictionary<string, object> { ["Type"] = "Demand_Spike", ["Severity"] = "High", ["Category"] = "Widgets" }), "ExternalInput: Demand_Spike (Widgets)"),
                (new BehaviorEvent("Conveyor_Main", "Throughput_Report", baseTime.AddMinutes(2), new Dictionary<string, object> { ["Value"] = 85, ["DeviationFromBaseline"] = -15, ["Unit"] = "units/min" }), "Conveyor_Main Throughput -15%")
            },
            VariantB => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("HVAC_Zone_A", "EnergyConsumption_Report", baseTime, new Dictionary<string, object> { ["Value"] = 120, ["DeviationFromBaseline"] = 25 }), "EnergyConsumption +25%"),
                (new BehaviorEvent("Conveyor_Main", "Throughput_Report", baseTime.AddMinutes(1), new Dictionary<string, object> { ["Value"] = 70, ["DeviationFromBaseline"] = -30 }), "Throughput bilinçli düşürülmüş -30%")
            },
            VariantC => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("Picking_Robot_2", "ErrorRate_Report", baseTime, new Dictionary<string, object> { ["Value"] = 0.02, ["DeviationFromBaseline"] = 0 }), "ErrorRate baseline"),
                (new BehaviorEvent("Conveyor_Main", "Throughput_Report", baseTime.AddMinutes(1), new Dictionary<string, object> { ["Value"] = 100, ["DeviationFromBaseline"] = 0 }), "Throughput baseline")
            },
            VariantD => new List<(BehaviorEvent, string)>
            {
                (new BehaviorEvent("Picking_Robot_2", "ErrorRate_Report", baseTime, new Dictionary<string, object> { ["Value"] = 0.40, ["DeviationFromBaseline"] = 4.0 }), "Picking_Robot_2 ErrorRate +400%"),
                (new BehaviorEvent("Conveyor_Main", "Throughput_Report", baseTime.AddMinutes(1), new Dictionary<string, object> { ["Value"] = 98, ["DeviationFromBaseline"] = -2 }), "Conveyor_Main normal"),
                (new BehaviorEvent("AGV_3", "Throughput_Report", baseTime.AddMinutes(2), new Dictionary<string, object> { ["Value"] = 100, ["DeviationFromBaseline"] = 0 }), "AGV_3 normal")
            },
            _ => GetEvents(VariantA, baseTime)
        };
    }
}
