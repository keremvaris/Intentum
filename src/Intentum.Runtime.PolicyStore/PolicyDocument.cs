using System.Text.Json;
using System.Text.Json.Serialization;

namespace Intentum.Runtime.PolicyStore;

/// <summary>
/// JSON document shape for intent policy (rules with declarative conditions).
/// </summary>
public sealed class PolicyDocument
{
    /// <summary>Version identifier (e.g. "1.0", "2024-01-15").</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    /// <summary>Ordered list of rules. First matching rule wins.</summary>
    [JsonPropertyName("rules")]
    public List<PolicyRuleDocument> Rules { get; set; } = [];
}

/// <summary>
/// Single rule in a policy document.
/// </summary>
public sealed class PolicyRuleDocument
{
    /// <summary>Rule name for explainability.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Decision when all conditions match (Allow, Block, Observe, Warn, Escalate, RequireAuth, RateLimit).</summary>
    [JsonPropertyName("decision")]
    public string Decision { get; set; } = "Observe";

    /// <summary>All conditions must match (AND). Allowed properties: intent.name, intent.confidence.level, intent.confidence.score.</summary>
    [JsonPropertyName("conditions")]
    public List<PolicyConditionDocument> Conditions { get; set; } = [];
}

/// <summary>
/// Declarative condition: property, operator, value. No arbitrary code.
/// </summary>
public sealed class PolicyConditionDocument
{
    /// <summary>Property path: intent.name, intent.confidence.level, intent.confidence.score.</summary>
    [JsonPropertyName("property")]
    public string Property { get; set; } = "";

    /// <summary>Operator: eq, ne, contains, gte, lte, gt, lt.</summary>
    [JsonPropertyName("operator")]
    public string Operator { get; set; } = "eq";

    /// <summary>Value (string or number). For intent.confidence.score use a number.</summary>
    [JsonPropertyName("value")]
    public JsonElement Value { get; set; }
}
