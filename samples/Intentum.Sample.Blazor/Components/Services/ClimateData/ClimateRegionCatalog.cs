namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

/// <summary>
/// Ülke bazlı iklim bölgesi profilleri kataloğu.
/// Koordinattan/ISO3'ten ülke eşleştirir ve bölgesel iklim karakterini döndürür.
/// Eksik veri olduğunda RiskCalculationEngine bu profili kullanır.
/// </summary>
public static class ClimateRegionCatalog
{
    private static readonly Dictionary<string, ClimateRegionProfile> Profiles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TUR"] = new ClimateRegionProfile
        {
            Code = "TUR", Name = "Türkiye",
            ClimateType = "Akdeniz/Karasal geçiş",
            DominantHazards = ["Kuraklık Riski", "Su Stresi", "Sıcaklık Artışı"],
            Seasonality = "İç bölgelerde yaz kuraklığı, kıyıda ani taşkın; Akdeniz yazın sıcak/kurak",
            CriticalInfrastructure = ["Su barajı bağımlılığı", "Sulama altyapısı", "Soğutma suyu"],
            AdaptiveRecommendations = ["Yağmur suyu hasadı", "Damla sulama", "Isı yalıtımı/gölgeleme"],
            BaselineWaterStress = 3.4, BaselineTempAnomaly = 2.0, BaselinePrecipChange = -15
        },
        ["USA"] = new ClimateRegionProfile
        {
            Code = "USA", Name = "ABD",
            ClimateType = "Çeşitli (kıyı/karasal/çöl)",
            DominantHazards = ["Aşırı sıcaklık", "Fırtına", "Su Stresi"],
            Seasonality = "Batıda kuraklık, Körfez kıyısında kasırga riski",
            CriticalInfrastructure = ["Soğutma suyu", "Enerji şebekesi", "Kıyı altyapısı"],
            AdaptiveRecommendations = ["Yedek jeneratör", "Kıyı koruma/drenaj", "Su geri dönüşümü"],
            BaselineWaterStress = 3.1, BaselineTempAnomaly = 1.8, BaselinePrecipChange = -8
        },
        ["DEU"] = new ClimateRegionProfile
        {
            Code = "DEU", Name = "Almanya",
            ClimateType = "Ilıman okyanusal",
            DominantHazards = ["Sel Riski", "Sıcaklık Artışı"],
            Seasonality = "Yaz aşırı sıcaklık, kış sel; nehir taşkını eğilimi",
            CriticalInfrastructure = ["Nehir taşkını kontrolü", "Sanayi soğutma"],
            AdaptiveRecommendations = ["Taşkın bariyeri", "Soğutma kapasite artırımı"],
            BaselineWaterStress = 2.2, BaselineTempAnomaly = 1.9, BaselinePrecipChange = -5
        },
        ["FRA"] = new ClimateRegionProfile
        {
            Code = "FRA", Name = "Fransa",
            ClimateType = "Ilıman/Akdeniz",
            DominantHazards = ["Sıcaklık Artışı", "Sel Riski"],
            Seasonality = "Güneyde Akdeniz kuraklığı, kuzeyde sel",
            CriticalInfrastructure = ["Nükleer soğutma suyu", "Tarım sulama"],
            AdaptiveRecommendations = ["Su tasarrufu", "Isı yalıtımı"],
            BaselineWaterStress = 2.8, BaselineTempAnomaly = 1.8, BaselinePrecipChange = -6
        },
        ["GBR"] = new ClimateRegionProfile
        {
            Code = "GBR", Name = "Birleşik Krallık",
            ClimateType = "Okyanusal",
            DominantHazards = ["Fırtına", "Sel Riski"],
            Seasonality = "Kıyı fırtına kabarması, kış sel",
            CriticalInfrastructure = ["Kıyı savunması", "Drenaj altyapısı"],
            AdaptiveRecommendations = ["Kıyı taşkın koruması", "Drenaj iyileştirme"],
            BaselineWaterStress = 1.8, BaselineTempAnomaly = 1.3, BaselinePrecipChange = 5
        },
        ["ITA"] = new ClimateRegionProfile
        {
            Code = "ITA", Name = "İtalya",
            ClimateType = "Akdeniz",
            DominantHazards = ["Kuraklık Riski", "Sıcaklık Artışı"],
            Seasonality = "Akdeniz yaz kuraklığı, ani sel",
            CriticalInfrastructure = ["Tarım sulama", "Turizm su ihtiyacı"],
            AdaptiveRecommendations = ["Yağmur suyu hasadı", "Su verimliliği"],
            BaselineWaterStress = 3.5, BaselineTempAnomaly = 2.1, BaselinePrecipChange = -12
        },
        ["ESP"] = new ClimateRegionProfile
        {
            Code = "ESP", Name = "İspanya",
            ClimateType = "Akdeniz/Kurak",
            DominantHazards = ["Kuraklık Riski", "Su Stresi"],
            Seasonality = "Yaz kuraklığı, su kıtlığı",
            CriticalInfrastructure = ["Su barajı", "Turizm/ziyaretçi su"],
            AdaptiveRecommendations = ["Tuzdan arındırma", "Su geri dönüşümü"],
            BaselineWaterStress = 4.0, BaselineTempAnomaly = 2.2, BaselinePrecipChange = -15
        },
        ["BRA"] = new ClimateRegionProfile
        {
            Code = "BRA", Name = "Brezilya",
            ClimateType = "Tropikal/Subtropikal",
            DominantHazards = ["Sel Riski", "Sıcaklık Artışı"],
            Seasonality = "Yoğun yağış mevsimi, Amazon kuraklık dönemi",
            CriticalInfrastructure = ["Hidroelektrik", "Tarım"],
            AdaptiveRecommendations = ["Drenaj iyileştirme", "Sel erken uyarı"],
            BaselineWaterStress = 2.0, BaselineTempAnomaly = 1.6, BaselinePrecipChange = -5
        },
        ["IND"] = new ClimateRegionProfile
        {
            Code = "IND", Name = "Hindistan",
            ClimateType = "Tropikal muson",
            DominantHazards = ["Sel Riski", "Aşırı sıcaklık"],
            Seasonality = "Muson seli, yaz aşırı sıcaklık",
            CriticalInfrastructure = ["Muson sulama", "Soğutma suyu"],
            AdaptiveRecommendations = ["Sel erken uyarı", "Isı yalıtımı"],
            BaselineWaterStress = 3.2, BaselineTempAnomaly = 1.7, BaselinePrecipChange = -7
        },
        ["CHN"] = new ClimateRegionProfile
        {
            Code = "CHN", Name = "Çin",
            ClimateType = "Çeşitli (kıyı/karasal)",
            DominantHazards = ["Sel Riski", "Kuraklık Riski"],
            Seasonality = "Yaz musonu seli, kuzey kuraklık",
            CriticalInfrastructure = ["Baraj", "Sanayi suyu"],
            AdaptiveRecommendations = ["Su tasarrufu", "Taşkın kontrolü"],
            BaselineWaterStress = 3.0, BaselineTempAnomaly = 1.9, BaselinePrecipChange = -6
        },
        ["JPN"] = new ClimateRegionProfile
        {
            Code = "JPN", Name = "Japonya",
            ClimateType = "Okyanusal/Adil",
            DominantHazards = ["Fırtına", "Sel Riski"],
            Seasonality = "Tayfun mevsimi, şiddetli yağış seli",
            CriticalInfrastructure = ["Kıyı savunması", "Tayfun hazırlığı"],
            AdaptiveRecommendations = ["Tayfun önlemleri", "Drenaj"],
            BaselineWaterStress = 1.9, BaselineTempAnomaly = 1.6, BaselinePrecipChange = -3
        }
    };

    private static readonly ClimateRegionProfile Default = new()
    {
        Code = "UNK", Name = "Belirsiz/Bölge Dışı",
        ClimateType = "Bilinmiyor",
        DominantHazards = ["Sıcaklık Artışı"],
        Seasonality = "Bölgesel profil mevcut değil — genel tahmin kullanılıyor",
        CriticalInfrastructure = ["—"],
        AdaptiveRecommendations = ["Lokasyon bazlı iklim verisi toplayın"],
        BaselineWaterStress = 2.5, BaselineTempAnomaly = 1.5, BaselinePrecipChange = -10
    };

    /// <summary>ISO3 ülke kodundan profil döndürür. Bilinmiyorsa varsayılan profil.</summary>
    public static ClimateRegionProfile Get(string iso3)
        => string.IsNullOrWhiteSpace(iso3) ? Default
           : Profiles.GetValueOrDefault(iso3, Default);

    /// <summary>Koordinattan ISO3 tespit edip profil döndürür.</summary>
    public static ClimateRegionProfile FromCoordinates(double lat, double lng)
        => Get(DetectIso3(lat, lng));

    /// <summary>Koordinattan yaklaşık ISO3 ülke kodu — sınır kutularıyla (mevcut DetectCountry mantığı).</summary>
    public static string DetectIso3(double lat, double lng)
    {
        if (lat > 36 && lat < 42 && lng > 26 && lng < 45) return "TUR";
        if (lat > 24 && lat < 50 && lng > -130 && lng < -65) return "USA";
        if (lat > 49 && lat < 61 && lng > -11 && lng < 2) return "GBR";
        if (lat > 47 && lat < 56 && lng > 5 && lng < 16) return "DEU";
        if (lat > 42 && lat < 52 && lng > -5 && lng < 10) return "FRA";
        if (lat > 36 && lat < 48 && lng > 6 && lng < 19) return "ITA";
        if (lat > 35.5 && lat < 44 && lng > -9.5 && lng < 4) return "ESP";
        if (lat > 18 && lat < 54 && lng > 73 && lng < 135) return "CHN";
        if (lat > 6 && lat < 38 && lng > 68 && lng < 98) return "IND";
        if (lat > 30 && lat < 46 && lng > 128 && lng < 146) return "JPN";
        if (lat > -35 && lat < 6 && lng > -75 && lng < -34) return "BRA";
        return "UNK";
    }
}
