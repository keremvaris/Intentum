namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

/// <summary>Coğrafi farkındalık: kıyıya uzaklık ve deniz seviyesi mantığı.</summary>
public static class GeoRiskHelper
{
    // Türkiye kıyılarına yaklaşık noktalar (Haversine ile en yakın mesafe)
    private static readonly (double lat, double lng)[] TurkeyCoastline =
    [
        (41.1, 28.5), (41.2, 30.5), (41.3, 33.0), (41.5, 35.0), (41.4, 38.5), // Karadeniz
        (40.9, 27.5), (40.3, 26.5), (39.5, 26.2), (38.5, 26.8), (37.8, 27.5), (36.8, 28.2), // Ege
        (36.7, 30.5), (36.4, 32.8), (36.0, 34.5), (36.2, 36.0), // Akdeniz
    ];

    private static readonly HashSet<string> InlandProvinceNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Ankara", "Konya", "Kayseri", "Eskisehir", "Sivas", "Nevsehir", "Kirsehir", "Aksaray",
        "Kutahya", "Afyonkarahisar", "Yozgat", "Corum", "Kirsehir", "Kirikkale", "Karaman"
    };

    public static (bool isCoastal, double distanceKm, string note) GetCoastalInfo(double lat, double lng, string locationName)
    {
        // İsim tabanlı hızlı kontrol
        if (!string.IsNullOrWhiteSpace(locationName) && InlandProvinceNames.Contains(locationName.Trim()))
            return (false, 280, "İç Anadolu'da — denize ~250-350km, doğrudan deniz seviyesi riski yok");

        // Türkiye bounding box dışındaysa: genel kural — deniz seviyesi riski düşük kabul et amanot bırak
        var isTurkeyLike = lat >= 35.5 && lat <= 42.5 && lng >= 25 && lng <= 45;
        if (!isTurkeyLike)
        {
            // Dünya geneli: kıyıya uzaklık kabaca 100km üstü ise iç bölge say
            // Basit: eğer açık okyanusta değilse kontrol edemiyoruz, deniz riski orta say
            return (true, 30, "Konum kıyıya yakın kabul edildi — deniz seviyesi etkisi dahil");
        }

        var minKm = double.MaxValue;
        foreach (var p in TurkeyCoastline)
            minKm = Math.Min(minKm, HaversineKm(lat, lng, p.lat, p.lng));

        // Karadeniz/Akdeniz/Ege için 80km üstü iç bölge
        var isCoastal = minKm <= 80;
        var note = isCoastal
            ? $"Kıyıya ~{minKm:F0}km — +{0.5:F1}m deniz seviyesi kıyı tesisleri için risk"
            : $"İç bölge (kıyıya ~{minKm:F0}km) — doğrudan deniz seviyesi riski yok, 50km yarıçap denize ulaşmaz";

        return (isCoastal, minKm, note);
    }

    public static double SeaLevelEffective(double seaLevelRise, bool isCoastal, double distanceKm)
    {
        if (!isCoastal || distanceKm > 120) return 0;
        if (distanceKm > 80) return seaLevelRise * 0.25; // geçiş zonu
        if (distanceKm > 40) return seaLevelRise * 0.6;
        return seaLevelRise;
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat/2)*Math.Sin(dLat/2) + Math.Cos(lat1*Math.PI/180)*Math.Cos(lat2*Math.PI/180)*Math.Sin(dLon/2)*Math.Sin(dLon/2);
        return 2 * R * Math.Asin(Math.Sqrt(a));
    }
}
