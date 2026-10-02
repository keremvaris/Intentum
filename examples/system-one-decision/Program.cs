// Intentum Example: System One decision engines as IIntentModel
// (Jev, Kev, Laya, TinyJev, OpenDecision, Decision 1.0, Open-Jev, NanoJev, OpenJevPro, Foq)
//
// Run:  dotnet run --project examples/system-one-decision -- --engine laya
// Env:  SYSTEMONE_ENGINE, SYSTEMONE_BASE_URL
//
// Start the engine first — see README.md for the launch command per engine.
// Jev is hosted: get a key at console.typesafe.ai/keys and export TYPESAFE_API_KEY.

using Intentum.AI.SystemOne;
using Intentum.Core.Behavior;

var engineName = GetArg("--engine") ?? Environment.GetEnvironmentVariable("SYSTEMONE_ENGINE") ?? "laya";
var baseUrl = GetArg("--base-url") ?? Environment.GetEnvironmentVariable("SYSTEMONE_BASE_URL");

SystemOneOptions options;
try
{
    options = SystemOneEngines.FromName(engineName, baseUrl);
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

options = options with
{
    IntentCatalog = new Dictionary<string, string>
    {
        ["Billing"] = "invoices, charges, refunds, payment problems",
        ["Technical"] = "bugs, outages, login failures, integration errors",
        ["Account"] = "profile, password reset, account recovery",
        ["Other"] = "everything else",
    },
};

var space = new BehaviorSpaceBuilder()
    .WithActor("user-42")
    .Action("payment.charged_twice")
    .Action("invoice.viewed")
    .Action("refund.requested")
    .Build();

Console.WriteLine($"Engine   : {options.Engine} ({options.BaseUrl})");
Console.WriteLine("Behavior :");
foreach (var behaviorEvent in space.Events)
    Console.WriteLine($"  {behaviorEvent.Actor}:{behaviorEvent.Action}");

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
var model = new SystemOneIntentModel(options, http);

try
{
    var intent = model.Infer(space);

    Console.WriteLine();
    Console.WriteLine($"Intent     : {intent.Name}");
    Console.WriteLine($"Confidence : {intent.Confidence.Score:0.00} ({intent.Confidence.Level})");
    Console.WriteLine($"Reasoning  : {intent.Reasoning}");
    foreach (var signal in intent.Signals.OrderByDescending(s => s.Weight))
        Console.WriteLine($"  signal   : {signal.Description} = {signal.Weight:0.00} [{signal.Source}]");

    return 0;
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine($"Cannot reach {options.Engine} at {options.BaseUrl}: {ex.Message}");
    Console.Error.WriteLine($"Hint: {LaunchHint(options.Engine)}");
    Console.Error.WriteLine("Or pass a different endpoint: --base-url http://127.0.0.1:8000");
    return 1;
}

string? GetArg(string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

static string LaunchHint(string engine) => engine switch
{
    "jev" => "Jev is hosted: export TYPESAFE_API_KEY from console.typesafe.ai/keys (endpoint https://api.typesafe.ai)",
    "kev" => "start the Kev server (serves /v1/systemone on 127.0.0.1:8009 — see github.com/jaredpalmer/kev)",
    "tinyjev" => "run `tinyjev serve TinyJev-0.6B` (serves on 127.0.0.1:8077)",
    "laya" => "run `pip install \"laya[serve]\" && laya-serve` (serves on 127.0.0.1:8000)",
    _ => $"start the {engine} server (POST /v1/systemone; default http://127.0.0.1:8000)",
};
