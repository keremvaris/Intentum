// Intentum Example: Behavior Vector Normalization
// Run: dotnet run --project examples/vector-normalization
// Shows ToVector(options): Cap, L1, SoftCap so dimension counts don't dominate.

using Intentum.Core;
using Intentum.Core.Behavior;

Console.WriteLine("=== Intentum Example: Vector Normalization ===\n");

var space = new BehaviorSpace()
    .Observe("user", "login.failed")
    .Observe("user", "login.failed")
    .Observe("user", "login.failed")
    .Observe("user", "login.failed")
    .Observe("user", "login.failed")
    .Observe("user", "password.reset")
    .Observe("user", "captcha.passed");

// Raw vector: user:login.failed = 5, user:password.reset = 1, user:captcha.passed = 1
var raw = space.ToVector();
Console.WriteLine("Raw (no normalization):");
foreach (var d in raw.Dimensions.OrderBy(x => x.Key))
    Console.WriteLine($"  {d.Key}: {d.Value}");
Console.WriteLine();

// Cap each dimension at 3
var capOptions = new ToVectorOptions(VectorNormalization.Cap, CapPerDimension: 3);
var capped = space.ToVector(capOptions);
Console.WriteLine("Cap per dimension (max 3):");
foreach (var d in capped.Dimensions.OrderBy(x => x.Key))
    Console.WriteLine($"  {d.Key}: {d.Value}");
Console.WriteLine();

// L1 norm: sum = 1
var l1Options = new ToVectorOptions(VectorNormalization.L1);
var l1 = space.ToVector(l1Options);
Console.WriteLine("L1 normalization (sum = 1):");
foreach (var d in l1.Dimensions.OrderBy(x => x.Key))
    Console.WriteLine($"  {d.Key}: {d.Value:F3}");
Console.WriteLine($"  Sum: {l1.Dimensions.Values.Sum():F3}");
Console.WriteLine();

// SoftCap: scale by cap (e.g. 5 → min(1, 5/3) = 1)
var softCapOptions = new ToVectorOptions(VectorNormalization.SoftCap, CapPerDimension: 3);
var softCapped = space.ToVector(softCapOptions);
Console.WriteLine("SoftCap (value/cap, min 1):");
foreach (var d in softCapped.Dimensions.OrderBy(x => x.Key))
    Console.WriteLine($"  {d.Key}: {d.Value:F3}");
Console.WriteLine();
Console.WriteLine("Use normalization so repeated events don't dominate; L1 or Cap for inference.");
