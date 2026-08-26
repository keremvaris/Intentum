using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Example.ClimateRisk.Intents;
using Intentum.Example.ClimateRisk.Models;
using Intentum.Example.ClimateRisk.Policy;
using Intentum.Example.ClimateRisk.Reports;
using Intentum.Example.ClimateRisk.Risks;
using Intentum.Example.ClimateRisk.Scenarios;
using Intentum.Runtime.Engine;

ConsoleReporter.PrintHeader();

var scenario = SspScenarios.Ssp2_45;
var sector = SectorProfile.Energy;
var horizon = TimeHorizon.MediumTerm2050;

ConsoleReporter.PrintScenario(scenario, sector, horizon);

var (physicalScore, physicalFactors) = PhysicalRiskCalculator.Calculate(scenario, sector, horizon);
var (transitionScore, transitionFactors) = TransitionRiskCalculator.Calculate(scenario, sector, horizon);

var space = new BehaviorSpace();
foreach (var f in physicalFactors)
    for (var i = 0; i < Math.Ceiling(f.WeightedScore * 5); i++)
        space.Observe("physical", f.Name.ToLowerInvariant());

foreach (var f in transitionFactors)
    for (var i = 0; i < Math.Ceiling(f.WeightedScore * 5); i++)
        space.Observe("transition", f.Name.ToLowerInvariant());

ConsoleReporter.PrintBehaviorSpace(space.Events.Count);

var model = new ClimateRiskIntentModel();
var intent = model.Infer(space);
ConsoleReporter.PrintIntent(intent);

var policy = ClimateRiskPolicy.Create();
var decision = IntentPolicyEngine.Evaluate(intent, policy);
ConsoleReporter.PrintPolicyDecision(decision, null);

ConsoleReporter.PrintPhysicalRisk(physicalScore, physicalFactors);
ConsoleReporter.PrintTransitionRisk(transitionScore, transitionFactors);

var economicImpact = EconomicImpactAnalyzer.Calculate(physicalScore, transitionScore, sector);
ConsoleReporter.PrintEconomicImpact(economicImpact);

var actions = GetActions(scenario, sector, physicalScore, transitionScore);
ConsoleReporter.PrintActions(actions);

if (args.Contains("--web"))
{
    var port = 5000;
    var portArg = args.FirstOrDefault(a => a.StartsWith("--port="));
    if (portArg != null) int.TryParse(portArg.Split('=')[1], out port);

    await WebUiHandler.StartServer(port);
}
else
{
    Console.WriteLine("Run with --web to start the web UI, or --port=XXXX to change the port.");
}

static IReadOnlyList<string> GetActions(ClimateScenario scenario, SectorProfile sector, double physical, double transition)
{
    var result = new List<string>();

    if (physical > 0.6)
    {
        result.Add($"Physical risk high for {sector.Name}: implement climate adaptation measures");
        result.Add("Develop business continuity plans for extreme weather events");
    }

    if (transition > 0.6)
    {
        result.Add($"Transition risk elevated: assess {sector.Name} exposure to policy and technology changes");
        result.Add("Develop transition roadmap aligned with scenario pathway");
    }

    if (physical > 0.4 && physical <= 0.6)
        result.Add("Monitor physical risk indicators quarterly");

    if (transition > 0.4 && transition <= 0.6)
        result.Add("Track regulatory developments and technology cost curves");

    if (result.Count == 0)
        result.Add("Continue standard risk monitoring procedures");

    return result;
}
