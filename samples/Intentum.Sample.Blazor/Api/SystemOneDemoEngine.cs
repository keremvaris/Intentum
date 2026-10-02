using System.Text.Json;
using Intentum.AI.SystemOne;

namespace Intentum.Sample.Blazor.Api;

/// <summary>
/// Deterministic in-process "engine" for POST /v1/systemone. Routes the observed
/// events to an intent via keyword rules; same input always yields same output.
/// </summary>
internal static class SystemOneDemoEngine
{
    private static readonly (string Intent, string[] Keywords)[] Rules =
    [
        ("Billing", ["refund", "payment", "invoice"]),
        ("Technical", ["login", "error", "bug", "outage"]),
        ("Account", ["password", "profile", "account"]),
    ];

    private const string FallbackIntent = "Other";

    internal static SystemOneResponse Decide(
        JsonElement state,
        IReadOnlyDictionary<string, SystemOneQuestion>? questions)
    {
        if (questions is null || !questions.TryGetValue("intent", out var question))
            return new SystemOneResponse();

        var options = ExtractChoiceKeys(question);
        if (options.Count == 0)
            return new SystemOneResponse();

        var events = ExtractEvents(state);
        var (name, confidence) = Resolve(events, options);

        return new SystemOneResponse
        {
            Answers = new Dictionary<string, SystemOneAnswer>
            {
                ["intent"] = new SystemOneAnswer
                {
                    Choice = name,
                    Confidence = confidence,
                    Probabilities = BuildProbabilities(options, name, confidence),
                },
            },
        };
    }

    private static (string Name, double Confidence) Resolve(IReadOnlyList<string> events, IReadOnlyList<string> options)
    {
        var matchedTotal = events.Count(HasAnyKeyword);
        if (matchedTotal == 0)
            return (Pick(FallbackIntent, options), 0.50);

        var best = (Intent: "", Count: 0);
        foreach (var (intent, keywords) in Rules)
        {
            var count = events.Count(e => keywords.Any(k => e.Contains(k, StringComparison.OrdinalIgnoreCase)));
            if (count > best.Count)
                best = (intent, count);
        }

        var confidence = Math.Min(0.97, 0.55 + 0.08 * matchedTotal);
        return (Pick(best.Intent, options), confidence);
    }

    private static bool HasAnyKeyword(string e) =>
        Rules.Any(r => r.Keywords.Any(k => e.Contains(k, StringComparison.OrdinalIgnoreCase)));

    private static string Pick(string name, IReadOnlyList<string> options) =>
        options.FirstOrDefault(o => string.Equals(o, name, StringComparison.OrdinalIgnoreCase))
        ?? options.First();

    private static Dictionary<string, double> BuildProbabilities(
        IReadOnlyList<string> options,
        string winner,
        double confidence)
    {
        var probabilities = new Dictionary<string, double>(StringComparer.Ordinal);
        var rest = (1.0 - confidence) / Math.Max(1, options.Count - 1);
        foreach (var option in options)
            probabilities[option] = option == winner ? confidence : Math.Round(rest, 6);
        return probabilities;
    }

    private static List<string> ExtractEvents(JsonElement state)
    {
        var events = new List<string>();
        if (state.ValueKind == JsonValueKind.String)
        {
            if (state.GetString() is { Length: > 0 } s)
                events.Add(s);
            return events;
        }

        if (state.ValueKind == JsonValueKind.Object
            && state.TryGetProperty("events", out var array)
            && array.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in array.EnumerateArray())
                if (item.ValueKind == JsonValueKind.String && item.GetString() is { } s)
                    events.Add(s);
        }

        return events;
    }

    private static List<string> ExtractChoiceKeys(SystemOneQuestion question) =>
        question.Criteria switch
        {
            // HTTP-bound path: Criteria arrives as raw JsonElement.
            JsonElement { ValueKind: JsonValueKind.Object } o =>
                o.EnumerateObject().Select(p => p.Name).ToList(),
            JsonElement { ValueKind: JsonValueKind.Array } a =>
                a.EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString()!)
                    .ToList(),
            // In-memory path (tests, direct calls): Criteria is the actual dictionary.
            IReadOnlyDictionary<string, string?> dictionary => dictionary.Keys.ToList(),
            IReadOnlyList<string> list => list.ToList(),
            IEnumerable<string> sequence => sequence.ToList(),
            _ => [],
        };
}
