using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Intentum.Core.Behavior;

/// <summary>
/// Options for sanitizing a behavior space (actor/action/metadata masking) for GDPR/CCPA or privacy.
/// </summary>
/// <param name="MaskActor">When true, replace actor with a hash or placeholder.</param>
/// <param name="MaskAction">When true, replace action with a hash or placeholder.</param>
/// <param name="MetadataKeysToRedact">Metadata keys to redact (value replaced with "[redacted]"). When null, no metadata redaction.</param>
/// <param name="UseHash">When true, use SHA256 hash (truncated) for masked values; when false, use Placeholder.</param>
/// <param name="Placeholder">Placeholder string when UseHash is false (e.g. "***").</param>
/// <param name="MaskEmails">When true, detect and mask email addresses in metadata values.</param>
/// <param name="MaskPhoneNumbers">When true, detect and mask phone numbers in metadata values.</param>
public sealed record SanitizationOptions(
    bool MaskActor = true,
    bool MaskAction = true,
    IReadOnlyList<string>? MetadataKeysToRedact = null,
    bool UseHash = true,
    string Placeholder = "***",
    bool MaskEmails = false,
    bool MaskPhoneNumbers = false);

/// <summary>
/// Extension methods for sanitizing (anonymizing/masking) a behavior space before vector production or inference.
/// Use for GDPR/CCPA compliance: mask PII in actor/action/metadata before sending to external models or storage.
/// </summary>
public static class BehaviorSpaceSanitization
{
    private static readonly Regex EmailRegex = new(@"[\w.+-]+@[\w-]+\.[\w.]+", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"[\+]?[\d\s\-\(\)]{10,}", RegexOptions.Compiled);
    /// <summary>
    /// Returns a new behavior space with actor/action/metadata sanitized according to options.
    /// Original space is not modified.
    /// </summary>
    public static BehaviorSpace Sanitize(this BehaviorSpace space, SanitizationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(space);
        options ??= new SanitizationOptions();
        var sanitized = new BehaviorSpace();

        foreach (var evt in space.Events)
        {
            var actor = options.MaskActor ? Mask(evt.Actor, options) : evt.Actor;
            var action = options.MaskAction ? Mask(evt.Action, options) : evt.Action;
            var meta = RedactMetadata(evt.Metadata, options.MetadataKeysToRedact, options);
            sanitized.Observe(new BehaviorEvent(actor, action, evt.OccurredAt, meta));
        }

        CopyMetadata(sanitized, space.Metadata, options.MetadataKeysToRedact);
        return sanitized;
    }

    private static IReadOnlyDictionary<string, object>? RedactMetadata(
        IReadOnlyDictionary<string, object>? meta,
        IReadOnlyList<string>? keysToRedact,
        SanitizationOptions options)
    {
        if (meta == null)
            return meta;
        var dict = new Dictionary<string, object>(meta);
        if (keysToRedact != null)
        {
            foreach (var key in keysToRedact.Where(dict.ContainsKey))
                dict[key] = "[redacted]";
        }
        if (options.MaskEmails || options.MaskPhoneNumbers)
        {
            foreach (var key in dict.Keys.ToList())
            {
                if (dict[key] is string value)
                    dict[key] = MaskPii(value, options);
            }
        }
        return dict;
    }

    private static string MaskPii(string value, SanitizationOptions options)
    {
        if (options.MaskEmails)
            value = EmailRegex.Replace(value, m => MaskEmail(m.Value));
        if (options.MaskPhoneNumbers)
            value = PhoneRegex.Replace(value, m => MaskPhone(m.Value));
        return value;
    }

    private static string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2) return "***";
        var maskedLocal = parts[0].Length > 0 ? parts[0][0].ToString() : "***";
        return $"{maskedLocal}@***.***";
    }

    private static string MaskPhone(string phone)
    {
        if (phone.Length < 4) return "***";
        return phone[..2] + "***" + phone[^2..];
    }

    private static void CopyMetadata(
        BehaviorSpace sanitized,
        IReadOnlyDictionary<string, object> metadata,
        IReadOnlyList<string>? keysToRedact)
    {
        foreach (var kv in metadata)
        {
            var value = keysToRedact != null && keysToRedact.Contains(kv.Key, StringComparer.OrdinalIgnoreCase)
                ? "[redacted]"
                : kv.Value;
            sanitized.SetMetadata(kv.Key, value);
        }
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
