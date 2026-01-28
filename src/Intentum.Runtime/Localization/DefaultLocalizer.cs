namespace Intentum.Runtime.Localization;

public sealed class DefaultLocalizer : IIntentumLocalizer
{
    private readonly IReadOnlyDictionary<string, string> _map;

    public DefaultLocalizer(string languageCode = "en")
    {
        _map = languageCode.ToLowerInvariant() switch
        {
            "tr" => Turkish(),
            _ => English()
        };
    }

    public string Get(string key)
        => _map.TryGetValue(key, out var value) ? value : key;

    private static Dictionary<string, string> English()
        => new Dictionary<string, string>
        {
            { LocalizationKeys.DecisionAllow, "Allow" },
            { LocalizationKeys.DecisionObserve, "Observe" },
            { LocalizationKeys.DecisionWarn, "Warn" },
            { LocalizationKeys.DecisionBlock, "Block" }
        };

    private static Dictionary<string, string> Turkish()
        => new Dictionary<string, string>
        {
            { LocalizationKeys.DecisionAllow, "İzin Ver" },
            { LocalizationKeys.DecisionObserve, "İzle" },
            { LocalizationKeys.DecisionWarn, "Uyar" },
            { LocalizationKeys.DecisionBlock, "Engelle" }
        };
}
