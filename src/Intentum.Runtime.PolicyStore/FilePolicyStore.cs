using System.Text.Json;
using Intentum.Runtime.Policy;
using Intentum.Versioning;

namespace Intentum.Runtime.PolicyStore;

/// <summary>
/// Loads intent policy from a JSON file. Supports version field for versioning.
/// </summary>
public sealed class FilePolicyStore : IPolicyStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly string _filePath;
    private readonly object _lock = new();
    private PolicyDocument? _cached;
    private DateTime _lastRead = DateTime.MinValue;

    /// <summary>
    /// Creates a file-based policy store. Optionally pass a path; default is "intent-policy.json" in current directory.
    /// </summary>
    public FilePolicyStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "intent-policy.json");
    }

    /// <inheritdoc />
    public Task<IntentPolicy> LoadAsync(CancellationToken cancellationToken = default)
    {
        var doc = LoadDocument();
        var policy = BuildPolicy(doc);
        return Task.FromResult(policy);
    }

    /// <inheritdoc />
    public Task<VersionedPolicy?> GetVersionAsync(string version, CancellationToken cancellationToken = default)
    {
        var doc = LoadDocument();
        if (!string.Equals(doc.Version, version, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<VersionedPolicy?>(null);
        var policy = BuildPolicy(doc);
        return Task.FromResult<VersionedPolicy?>(new VersionedPolicy(doc.Version, policy));
    }

    private PolicyDocument LoadDocument()
    {
        lock (_lock)
        {
            var lastWrite = File.Exists(_filePath) ? File.GetLastWriteTimeUtc(_filePath) : DateTime.MinValue;
            if (_cached != null && _lastRead >= lastWrite)
                return _cached;

            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Policy file not found: {_filePath}");

            var json = File.ReadAllText(_filePath);
            _cached = JsonSerializer.Deserialize<PolicyDocument>(json, JsonOptions)
                ?? new PolicyDocument();
            _lastRead = DateTime.UtcNow;
            return _cached;
        }
    }

    private static IntentPolicy BuildPolicy(PolicyDocument doc)
    {
        var policy = new IntentPolicy();
        foreach (var ruleDoc in doc.Rules)
        {
            if (string.IsNullOrWhiteSpace(ruleDoc.Name))
                continue;

            var decision = Enum.TryParse<PolicyDecision>(ruleDoc.Decision, true, out var d) ? d : PolicyDecision.Observe;
            var condition = SafeConditionBuilder.Build(ruleDoc.Conditions);
            policy.AddRule(new PolicyRule(ruleDoc.Name, condition, decision));
        }
        return policy;
    }
}
