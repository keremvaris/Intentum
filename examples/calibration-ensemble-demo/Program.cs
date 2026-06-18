using Intentum.AI.Calibration;
using Intentum.AI.Ensemble;
using Intentum.Core.Behavior;
using Intentum.Core.Intents;
using Intentum.Core.Contracts;

Console.WriteLine("=== Calibration & Ensemble Demo ===\n");

// 1. Platt Calibration
Console.WriteLine("--- Platt Calibration ---");
var platt = new PlattCalibrator(a: 2.0, b: -1.0);
var rawScores = new[] { 0.1, 0.3, 0.5, 0.7, 0.9 };
foreach (var raw in rawScores)
{
    var calibrated = platt.Calibrate(raw);
    Console.WriteLine($"  Raw: {raw:F2} -> Calibrated: {calibrated:F4}");
}

// 2. Temperature Calibration
Console.WriteLine("\n--- Temperature Scaling ---");
var tempLow = new TemperatureCalibrator(temperature: 0.5);
var tempHigh = new TemperatureCalibrator(temperature: 5.0);
Console.WriteLine("  High temp (softer):");
foreach (var raw in rawScores)
    Console.WriteLine($"    Raw: {raw:F2} -> T=5: {tempHigh.Calibrate(raw):F4}");
Console.WriteLine("  Low temp (sharper):");
foreach (var raw in rawScores)
    Console.WriteLine($"    Raw: {raw:F2} -> T=0.5: {tempLow.Calibrate(raw):F4}");

// 3. Weighted Ensemble
Console.WriteLine("\n--- Weighted Ensemble ---");
var weighted = new WeightedEnsemble();
var results = new[]
{
    new ModelResult("Purchase", 0.9, 1.0),
    new ModelResult("Browse", 0.4, 0.5),
    new ModelResult("Purchase", 0.8, 1.0)
};
var intent = weighted.Combine(results);
Console.WriteLine($"  Ensemble: {intent.Name}, score: {intent.Confidence.Score:F2}");

// 4. Majority Voting
Console.WriteLine("\n--- Majority Voting ---");
var majority = new MajorityVotingEnsemble();
var tieResults = new[]
{
    new ModelResult("Purchase", 0.6, 1.0),
    new ModelResult("Support", 0.95, 1.0)
};
var tieIntent = majority.Combine(tieResults);
Console.WriteLine($"  Tie-breaker: {tieIntent.Name} (highest confidence at {tieIntent.Confidence.Score:F2})");

Console.WriteLine("\n=== Demo Complete ===");
