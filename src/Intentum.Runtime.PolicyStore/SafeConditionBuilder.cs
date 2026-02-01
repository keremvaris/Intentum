using System.Text.Json;
using Intentum.Core.Intents;

namespace Intentum.Runtime.PolicyStore;

/// <summary>
/// Builds Func&lt;Intent, bool&gt; from declarative conditions. Only allows intent.name, intent.confidence.level, intent.confidence.score.
/// </summary>
public static class SafeConditionBuilder
{
    private static readonly HashSet<string> AllowedProperties = ["intent.name", "intent.confidence.level", "intent.confidence.score"];
    private static readonly HashSet<string> AllowedOperators = ["eq", "ne", "contains", "gte", "lte", "gt", "lt"];

    /// <summary>
    /// Builds a condition from a list of declarative conditions (AND). Throws if property or operator is not allowed.
    /// </summary>
    public static Func<Intent, bool> Build(IReadOnlyList<PolicyConditionDocument> conditions)
    {
        if (conditions.Count == 0)
            return _ => true;

        var predicates = new List<Func<Intent, bool>>();
        foreach (var c in conditions)
        {
            var prop = c.Property.Trim().ToLowerInvariant();
            if (!AllowedProperties.Contains(prop))
                throw new ArgumentException($"Policy condition property not allowed: {c.Property}. Allowed: intent.name, intent.confidence.level, intent.confidence.score.", nameof(conditions));

            var op = c.Operator.Trim().ToLowerInvariant();
            if (!AllowedOperators.Contains(op))
                throw new ArgumentException($"Policy condition operator not allowed: {c.Operator}. Allowed: eq, ne, contains, gte, lte, gt, lt.", nameof(conditions));

            predicates.Add(BuildSingle(prop, op, c.Value));
        }

        return intent =>
        {
            foreach (var p in predicates)
            {
                if (!p(intent))
                    return false;
            }
            return true;
        };
    }

    private static Func<Intent, bool> BuildSingle(string property, string op, JsonElement value)
    {
        return property switch
        {
            "intent.name" => intent => CompareString(intent.Name, op, value),
            "intent.confidence.level" => intent => CompareString(intent.Confidence.Level, op, value),
            "intent.confidence.score" => intent => CompareScore(intent.Confidence.Score, op, value),
            _ => _ => false
        };
    }

    private static bool CompareString(string actual, string op, JsonElement value)
    {
        var expected = value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : value.GetRawText().Trim('"');
        return op switch
        {
            "eq" => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            "ne" => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            "contains" => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static bool CompareScore(double actual, string op, JsonElement value)
    {
        var expected = value.ValueKind == JsonValueKind.Number ? value.GetDouble() : 0d;
        if (value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(), out var parsed))
            expected = parsed;

        return op switch
        {
            "eq" => Math.Abs(actual - expected) < 1e-9,
            "ne" => Math.Abs(actual - expected) >= 1e-9,
            "gte" => actual >= expected,
            "lte" => actual <= expected,
            "gt" => actual > expected,
            "lt" => actual < expected,
            _ => false
        };
    }
}
