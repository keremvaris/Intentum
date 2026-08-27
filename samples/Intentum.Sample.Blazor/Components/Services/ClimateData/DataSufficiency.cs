namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

/// <summary>
/// Veri yeterlilik katmanı: her veri kaynağının mevcudiyetini izler,
/// eksik kaynakları tespit eder ve 0-1 arası bir DataConfidence üretir.
/// Amaç: "veri yok"u "düşük risk"le karıştırmamak — eksik bilgiyi açıkça işaretlemek.
/// </summary>
public static class DataSufficiency
{
    public static DataConfidenceResult Evaluate(
        bool hasProjection,
        bool hasWri,
        bool hasCompanyProfile,
        bool hasDistrictData = true)
    {
        var missing = new List<string>();

        if (!hasProjection) missing.Add("Open-Meteo projeksiyon");
        if (!hasWri) missing.Add("WRI su stresi");
        if (!hasCompanyProfile) missing.Add("Şirket finansal profili");
        if (!hasDistrictData) missing.Add("GADM ilçe verisi");

        // Zorunlu 3 kaynağın mevcudiyeti; ilçe verisi opsiyonel katkı sağlamaz.
        var required = new[] { hasProjection, hasWri, hasCompanyProfile };
        var score = (double)required.Count(x => x) / required.Length;

        return new DataConfidenceResult
        {
            Score = Math.Clamp(score, 0, 1),
            Missing = missing,
            IsRegionalEstimate = !hasWri || !hasProjection
        };
    }
}

public sealed class DataConfidenceResult
{
    public double Score { get; init; }
    public List<string> Missing { get; init; } = [];
    public bool IsRegionalEstimate { get; init; }
}
