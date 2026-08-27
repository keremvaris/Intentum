namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

/// <summary>
/// Bir ülkenin/coğrafi bölgenin iklim riski karakteri.
/// Tam iklim külliyatı yerine ülkenin bilinen iklim profilini kullanır —
/// eksik veri olduğunda (örn. WRI ülke verisi yok) bölgesel tahmin üretir.
/// Böylece kullanıcı veri yüklemek zorunda kalmaz; sistem bölgeden çıkarım yapar.
/// </summary>
public sealed class ClimateRegionProfile
{
    /// <summary>ISO3 ülke kodu veya bölge anahtarı (örn. "TUR", "USA").</summary>
    public string Code { get; set; } = "";

    /// <summary>Görünen ad (örn. "Türkiye").</summary>
    public string Name { get; set; } = "";

    /// <summary>Kıyı / iç karma karakteri.</summary>
    public bool IsPrimarilyCoastal { get; set; }

    /// <summary>İklim karakteri ana kategorisi (Akdeniz, Karadeniz, Karasal, Tropikal, Kurak, Soğuk...).</summary>
    public string ClimateType { get; set; } = "";

    /// <summary>Baskın tehlikeler — RiskMatrixEngine'deki tehlike isimleriyle eşleşir.</summary>
    public List<string> DominantHazards { get; set; } = [];

    /// <summary>Mevsimsellik / tetikleyici (örn. "Akdeniz yaz kuraklığı", "Karadeniz ani taşkın").</summary>
    public string Seasonality { get; set; } = "";

    /// <summary>Kritik altyapı bağımlılıkları (örn. su barajı, soğutma suyu).</summary>
    public List<string> CriticalInfrastructure { get; set; } = [];

    /// <summary>Bölgeye özgü adaptif kapasite önerileri (örn. yağmur suyu hasadı, gölgeleme).</summary>
    public List<string> AdaptiveRecommendations { get; set; } = [];

    /// <summary>Su stresi baz değeri (0-5). WRI ülke verisi yoksa kullanılır.</summary>
    public double BaselineWaterStress { get; set; } = 2.5;

    /// <summary>Sıcaklık anomalisi baz değeri (°C). Open-Meteo yoksa kullanılır.</summary>
    public double BaselineTempAnomaly { get; set; } = 1.5;

    /// <summary>Yağış değişimi baz değeri (%). Open-Meteo yoksa kullanılır.</summary>
    public double BaselinePrecipChange { get; set; } = -10;
}
