using System.Security.Cryptography;
using System.Text;

namespace Intentum.Core.Behavior;

/// <summary>
/// Options for sanitizing a behavior space (actor/action/metadata masking) for GDPR/CCPA or privacy.
/// </summary>
/// <param name="MaskActor">When true, replace actor with a hash or placeholder.</param>
/// <param name="MaskAction">When true, replace action with a hash or placeholder.</param>
/// <param name="MetadataKeysToRedact">Metadata keys to redact (value replaced with "[redacted]"). When null, no metadata redaction.</param>
/// <param name="UseHash">When true, use SHA256 hash (truncated) for masked values; when false, use Placeholder.</param>
/// <param name="Placeholder">Placeholder string when UseHash is false (e.g. "***").</param>
public sealed record SanitizationOptions(
    bool MaskActor = true,
    bool MaskAction = true,
    IReadOnlyList<string>? MetadataKeysToRedact = null,
    bool UseHash = true,
    string Placeholder = "***");

/// <summary>
/// Extension methods for sanitizing (anonymizing/masking) a behavior space before vector production or inference.
/// Use for GDPR/CCPA compliance: mask PII in actor/action/metadata before sending to external models or storage.
/// </summary>
public static class BehaviorSpaceSanitization
{
    /// <summary>
    /// Returns a new behavior space with actor/action/metadata sanitized according to options.
    /// Original space is not modified.
    /// </summary>
    public static BehaviorSpace Sanitize(this BehaviorSpace space, SanitizationOptions? options = null)
    {
        options ??= new SanitizationOptions();
        var sanitized = new BehaviorSpace();

        foreach (var evt in space.Events)
        {
            var actor = options.MaskActor ? Mask(evt.Actor, options) : evt.Actor;
            var action = options.MaskAction ? Mask(evt.Action, options) : evt.Action;
            var meta = evt.Metadata;
            if (options.MetadataKeysToRedact != null && meta != null)
            {
                var dict = new Dictionary<string, object>(meta);
                foreach (var key in options.MetadataKeysToRedact)
                {
                    if (dict.ContainsKey(key))
                        dict[key] = "[redacted]";
                }
                meta = dict;
            }
            sanitized.Observe(new BehaviorEvent(actor, action, evt.OccurredAt, meta));
        }

        foreach (var kv in space.Metadata)
        {
            var value = options.MetadataKeysToRedact != null && options.MetadataKeysToRedact.Contains(kv.Key, StringComparer.OrdinalIgnoreCase)
                ? "[redacted]"
                : kv.Value;
            sanitized.SetMetadata(kv.Key, value);
        }

        return sanitized;
    }

    private static string Mask(string value, SanitizationOptions options)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        if (options.UseHash)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes)[..Math.Min(16, bytes.Length * 2)];
        }
        return options.Placeholder;
    }
}
