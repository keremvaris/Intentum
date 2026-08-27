namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

/// <summary>
/// Bütün Sistemden Etkilenecek Şeyler modeli: İklim risk analizinin şirketin tüm sistemlerine
/// yayılacak etkilerini kategorize eder.
/// </summary>
public sealed class SystemImpactResult
{
    public List<SystemImpactCategory> Categories { get; set; } = [];
    public string Summary { get; set; } = "";
    public int TotalImpacts { get; set; }
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
}

public sealed class SystemImpactCategory
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public List<SystemImpactItem> Items { get; set; } = [];
}

public sealed class SystemImpactItem
{
    public string Description { get; set; } = "";
    public string Urgency { get; set; } = ""; // CRITICAL, HIGH, MEDIUM, LOW
    public string Trigger { get; set; } = "";  // hangi koşul tetikledi
    public string Timeline { get; set; } = ""; // ne zaman harekete geçilmeli
}

public static class SystemImpactModel
{
    public static SystemImpactResult Calculate(
        string decision,
        double physical, double transition,
        double waterStress,
        (bool isCoastal, double distanceKm, string note) coastal,
        WriCountryRisk? wri,
        FinancialImpact? financialImpact,
        CompanyProfile? companyProfile,
        string sector,
        ClimateRegionProfile? region,
        List<string> dominantHazards)
    {
        var result = new SystemImpactResult();

        // 1. Operasyonel Sistemler
        result.Categories.Add(BuildOperationalImpacts(decision, physical, waterStress, coastal, wri, sector));

        // 2. Tedarik Zinciri
        result.Categories.Add(BuildSupplyChainImpacts(decision, physical, waterStress, sector, dominantHazards));

        // 3. Finansal Sistemler
        result.Categories.Add(BuildFinancialImpacts(decision, financialImpact, companyProfile, physical));

        // 4. Yasal/Regülasyon
        result.Categories.Add(BuildRegulatoryImpacts(decision, transition, sector));

        // 5. İnsan Kaynakları
        result.Categories.Add(BuildHRImpacts(decision, physical, coastal, dominantHazards));

        // 6. Altyapı/Yatırım
        result.Categories.Add(BuildInfrastructureImpacts(decision, physical, waterStress, coastal, dominantHazards));

        // 7. Paydaşlar
        result.Categories.Add(BuildStakeholderImpacts(decision, financialImpact, companyProfile));

        // 8. Teknoloji/Bilişim
        result.Categories.Add(BuildTechImpacts(decision, physical, transition));

        // Özet hesapla
        result.TotalImpacts = result.Categories.Sum(c => c.Items.Count);
        result.CriticalCount = result.Categories.SelectMany(c => c.Items).Count(i => i.Urgency == "CRITICAL");
        result.HighCount = result.Categories.SelectMany(c => c.Items).Count(i => i.Urgency == "HIGH");
        result.Summary = BuildSummary(result, decision, companyProfile?.Name ?? "Şirket");

        return result;
    }

    private static SystemImpactCategory BuildOperationalImpacts(string decision, double physical, double waterStress, (bool isCoastal, double distanceKm, string note) coastal, WriCountryRisk? wri, string sector)
    {
        var items = new List<SystemImpactItem>();
        var urgency = MapDecisionToUrgency(decision);

        if (physical > 0.6)
            items.Add(new() { Description = "Üretim hatları ve makine parkurları — aşırı sıcaklık/kuraklık nedeniyle duruş süresi artabilir", Urgency = urgency, Trigger = $"Fiziksel risk {physical:F2}", Timeline = urgency == "CRITICAL" ? "1-3 ay" : "3-6 ay" });

        if (waterStress >= 3)
            items.Add(new() { Description = "Su tüketimi yüksek süreçler (soğutma, temizleme, proses suyu) — su kıstı nedeniyle kota uğrayabilir", Urgency = "HIGH", Trigger = $"Su stresi {waterStress:F1}/5", Timeline = "3-6 ay" });

        if (sector == "Enerji")
            items.Add(new() { Description = "Enerji üretim kapasitesi — soğutma suyu kısıtı ve yüksek sıcaklık verimliliği düşürür", Urgency = urgency, Trigger = "Enerji sektörü + yüksek fiziksel risk", Timeline = "1-3 ay" });

        if (sector == "Tarım")
            items.Add(new() { Description = "Sulama altyapısı ve ürün verimi — kuraklık doğrudan verimi etkiler", Urgency = "HIGH", Trigger = "Tarım sektörü + su stresi", Timeline = "Mevsimsel (hemen)" });

        if (coastal.isCoastal && physical > 0.5)
            items.Add(new() { Description = "Kıyı tesisleri — taşkın/sel nedeniyle erişim ve operasyon kesintisi", Urgency = "HIGH", Trigger = "Kıyı konumu + fiziksel risk", Timeline = "3-6 ay" });

        if (items.Count == 0)
            items.Add(new() { Description = "Mevcut operasyonel süreçlerde doğrudan kritik etki beklenmiyor — standart izleme yeterli", Urgency = "LOW", Trigger = "Düşük risk skorları", Timeline = "Yıllık" });

        return new SystemImpactCategory { Name = "Operasyonel Sistemler", Icon = "⚙️", Items = items };
    }

    private static SystemImpactCategory BuildSupplyChainImpacts(string decision, double physical, double waterStress, string sector, List<string> dominantHazards)
    {
        var items = new List<SystemImpactItem>();

        if (physical > 0.5 || waterStress >= 3)
        {
            items.Add(new() { Description = "Tedarikçi değerlendirmesi — iklim riski yüksek tedarikçiler alternatif listede tutulmalı", Urgency = "HIGH", Trigger = "Fiziksel risk/su stresi yüksek", Timeline = "3-6 ay" });

            if (sector == "Üretim" || sector == "Enerji" || sector == "Sanayi")
                items.Add(new() { Description = "Hammaddede fiyat dalgalanması — kuraklık/taşkın hammadde arzını ve fiyatlarını etkiler", Urgency = MapDecisionToUrgency(decision), Trigger = "Baskın tehlikeler: " + string.Join(", ", dominantHazards.Take(2)), Timeline = "6-12 ay" });
        }

        if (dominantHazards.Contains("Kuraklık Riski") || dominantHazards.Contains("Su Stresi"))
            items.Add(new() { Description = "Su bağımlı tedarikçiler — su kıstı tedarik kesintilerine yol açabilir", Urgency = "MEDIUM", Trigger = "Bölgesel kuraklık riski", Timeline = "6-12 ay" });

        if (items.Count == 0)
            items.Add(new() { Description = "Tedarik zincirinde doğrudan kritik etki beklenmiyor", Urgency = "LOW", Trigger = "Düşük risk", Timeline = "Yıllık" });

        return new SystemImpactCategory { Name = "Tedarik Zinciri", Icon = "🔗", Items = items };
    }

    private static SystemImpactCategory BuildFinancialImpacts(string decision, FinancialImpact? financialImpact, CompanyProfile? companyProfile, double physical)
    {
        var items = new List<SystemImpactItem>();

        if (financialImpact != null && financialImpact.NetCashFlowImpact < -10_000_000)
            items.Add(new() { Description = $"Nakit akışı planlaması — beklenen kayıp {financialImpact.NetCashFlowImpact:N0} TL/yıl, yedek fon ayrılmalı", Urgency = "CRITICAL", Trigger = "Kritik finansal maruziyet", Timeline = "Hemen (1 ay)" });

        if (financialImpact != null && financialImpact.NetCashFlowImpact < -1_000_000 && financialImpact.NetCashFlowImpact >= -10_000_000)
            items.Add(new() { Description = $"Bütçe revizyonu — net kayıp {financialImpact.NetCashFlowImpact:N0} TL/yıl, Operational gider kalemleri yeniden değerlendirilmeli", Urgency = "HIGH", Trigger = "Yüksek finansal maruziyet", Timeline = "1-3 ay" });

        if (physical > 0.5 && companyProfile != null)
            items.Add(new() { Description = "Sigorta poliçesi kapsamı — mevcut teminatlar iklim risklerini karşılayacak mı?", Urgency = "HIGH", Trigger = "Fiziksel risk yüksek + şirket profili mevcut", Timeline = "3-6 ay" });

        if (companyProfile != null)
            items.Add(new() { Description = "Yatırım planları —CAPEX kararlarında iklim riskini dikkate alın, yüksek riskli bölgelere yeni yatırım erteleyin", Urgency = MapDecisionToUrgency(decision), Trigger = "Şirket profili mevcut", Timeline = "6-12 ay" });

        if (items.Count == 0)
            items.Add(new() { Description = "Finansal sistemlerde doğrudan kritik etki beklenmiyor", Urgency = "LOW", Trigger = "Düşük finansal maruziyet", Timeline = "Yıllık" });

        return new SystemImpactCategory { Name = "Finansal Sistemler", Icon = "💰", Items = items };
    }

    private static SystemImpactCategory BuildRegulatoryImpacts(string decision, double transition, string sector)
    {
        var items = new List<SystemImpactItem>();

        if (transition > 0.5)
            items.Add(new() { Description = "TCFD/TNFD raporlama gereksinimleri — finansal olmayan raporlama zorunluluğu artabilir", Urgency = "HIGH", Trigger = $"Geçiş riski {transition:F2}", Timeline = "6-12 ay" });

        if (sector == "Enerji" || sector == "Sanayi")
            items.Add(new() { Description = "Emisyon izinleri ve karbon vergisi — Sınırda Karbon Düzenlemesi (CBAM) uyum hazırlığı", Urgency = transition > 0.6 ? "HIGH" : "MEDIUM", Trigger = "Sektörel regülasyon riski", Timeline = "12-24 ay" });

        if (decision == "REJECT")
            items.Add(new() { Description = "Yönetim kurulu bilgilendirme — kritik iklim riski yasal bildirim yükümlülüğü doğurabilir", Urgency = "CRITICAL", Trigger = "REJECT kararı", Timeline = "Hemen" });

        if (items.Count == 0)
            items.Add(new() { Description = "Mevcut regülasyon yükümlülükleri standart seviyede", Urgency = "LOW", Trigger = "Düşük geçiş riski", Timeline = "Yıllık" });

        return new SystemImpactCategory { Name = "Yasal/Regülasyon", Icon = "⚖️", Items = items };
    }

    private static SystemImpactCategory BuildHRImpacts(string decision, double physical, (bool isCoastal, double distanceKm, string note) coastal, List<string> dominantHazards)
    {
        var items = new List<SystemImpactItem>();

        if (physical > 0.6)
            items.Add(new() { Description = "İşçi sağlığı ve güvenliği — aşırı sıcaklık protokolleri, yaz çalışma planlaması, soğutma ekipmanı", Urgency = "HIGH", Trigger = $"Fiziksel risk {physical:F2}", Timeline = "1-3 ay" });

        if (dominantHazards.Contains("Isı Stresi") || dominantHazards.Contains("Sıcaklık Artışı"))
            items.Add(new() { Description = "Dış mekan çalışma saatleri — yaz aylarında 12:00-15:00 arası çalışma kısıtlaması düşünülebilir", Urgency = "MEDIUM", Trigger = "Bölgesel ısı stresi", Timeline = "Mevsimsel" });

        if (decision == "REJECT")
            items.Add(new() { Description = "Acil durum tahliye eğitimi — tesis personeli için iklim acil durum tatbikatı", Urgency = "CRITICAL", Trigger = "REJECT kararı", Timeline = "1 ay" });

        if (items.Count == 0)
            items.Add(new() { Description = "İnsan kaynakları süreçlerinde doğrudan kritik etki beklenmiyor", Urgency = "LOW", Trigger = "Düşük fiziksel risk", Timeline = "Yıllık" });

        return new SystemImpactCategory { Name = "İnsan Kaynakları", Icon = "👥", Items = items };
    }

    private static SystemImpactCategory BuildInfrastructureImpacts(string decision, double physical, double waterStress, (bool isCoastal, double distanceKm, string note) coastal, List<string> dominantHazards)
    {
        var items = new List<SystemImpactItem>();

        if (physical > 0.6)
            items.Add(new() { Description = "Tesis altyapısı dayanıklılık testi — bina, çatı, elektrik, soğutma sistemleri gözden geçirilmeli", Urgency = "HIGH", Trigger = $"Fiziksel risk {physical:F2}", Timeline = "3-6 ay" });

        if (coastal.isCoastal && physical > 0.4)
            items.Add(new() { Description = "Deniz seviyesi yükselmelerine karşı tesis kotu ve drenaj altyapısı — +0.5m/+1.0m senaryoları", Urgency = "HIGH", Trigger = "Kıyı konumu + fiziksel risk", Timeline = "6-12 ay" });

        if (waterStress >= 4)
            items.Add(new() { Description = "Alternatif su kaynakları — geri kazanılmış su, yağmur suyu hasadı, kuyu suyu fizibilitesi", Urgency = "HIGH", Trigger = $"Çok yüksek su stresi ({waterStress:F1}/5)", Timeline = "3-6 ay" });

        if (waterStress >= 3 && waterStress < 4)
            items.Add(new() { Description = "Su verimliliği yatırımları — damla sulama, geri dönüşüm sistemi, su sayacı kurulumu", Urgency = "MEDIUM", Trigger = $"Yüksek su stresi ({waterStress:F1}/5)", Timeline = "6-12 ay" });

        if (decision == "REJECT")
            items.Add(new() { Description = "Acil altyapı güçlendirme — yedek enerji, yedek su, acil durum jeneratörü", Urgency = "CRITICAL", Trigger = "REJECT kararı", Timeline = "1-3 ay" });

        if (items.Count == 0)
            items.Add(new() { Description = "Altyapıda doğrudan kritik yatırım ihtiyacı beklenmiyor", Urgency = "LOW", Trigger = "Düşük risk", Timeline = "Yıllık" });

        return new SystemImpactCategory { Name = "Altyapı/Yatırım", Icon = "🏗️", Items = items };
    }

    private static SystemImpactCategory BuildStakeholderImpacts(string decision, FinancialImpact? financialImpact, CompanyProfile? companyProfile)
    {
        var items = new List<SystemImpactItem>();

        if (decision == "REJECT")
        {
            items.Add(new() { Description = "Yönetim kurulu / üst yönetim bilgilendirme — kritik risk raporu sunulmalı", Urgency = "CRITICAL", Trigger = "REJECT kararı", Timeline = "Hemen" });
            items.Add(new() { Description = "Paydaş iletişimi — yatırımcılar, ortaklar ve müşterilere iklim riski hakkında bilgilendirme", Urgency = "HIGH", Trigger = "REJECT kararı", Timeline = "1 ay" });
        }

        if (financialImpact != null && financialImpact.NetCashFlowImpact < -10_000_000)
            items.Add(new() { Description = "Kredi verenler / banka ilişkileri — finansal etki assessment paylaşılmalı", Urgency = "HIGH", Trigger = "Kritik finansal maruziyet", Timeline = "1-3 ay" });

        if (companyProfile != null && decision != "ALLOW")
            items.Add(new() { Description = "Müşteri iletişimi — operasyonel süreklilik planı paylaşılmalı", Urgency = "MEDIUM", Trigger = "REVIEW/REJECT kararı", Timeline = "3-6 ay" });

        if (items.Count == 0)
            items.Add(new() { Description = "Paydaş iletişiminde acil eylem beklenmiyor", Urgency = "LOW", Trigger = "ALLOW kararı", Timeline = "Yıllık" });

        return new SystemImpactCategory { Name = "Paydaşlar", Icon = "🤝", Items = items };
    }

    private static SystemImpactCategory BuildTechImpacts(string decision, double physical, double transition)
    {
        var items = new List<SystemImpactItem>();

        if (physical > 0.5)
            items.Add(new() { Description = "Veri merkezi / BT altyapısı — soğutma gereksinimleri ve kesinti riski artar", Urgency = "HIGH", Trigger = $"Fiziksel risk {physical:F2}", Timeline = "3-6 ay" });

        if (transition > 0.5)
            items.Add(new() { Description = "Karbon izleme ve raporlama sistemleri — GHG protokolü uyumlu veri toplama altyapısı", Urgency = "MEDIUM", Trigger = $"Geçiş riski {transition:F2}", Timeline = "6-12 ay" });

        if (decision == "REJECT")
            items.Add(new() { Description = "İklim risk izleme dashboard'u — gerçek zamanlı veri entegrasyonu ve otomatik alarm sistemi", Urgency = "HIGH", Trigger = "REJECT kararı", Timeline = "1-3 ay" });

        if (items.Count == 0)
            items.Add(new() { Description = "BT sistemlerinde doğrudan kritik etki beklenmiyor", Urgency = "LOW", Trigger = "Düşük risk", Timeline = "Yıllık" });

        return new SystemImpactCategory { Name = "Teknoloji/Bilişim", Icon = "💻", Items = items };
    }

    private static string BuildSummary(SystemImpactResult result, string decision, string companyName)
    {
        var criticalText = result.CriticalCount > 0 ? $"{result.CriticalCount} kritik" : "";
        var highText = result.HighCount > 0 ? $"{result.HighCount} yüksek" : "";
        var severity = string.Join(", ", new[] { criticalText, highText }.Where(s => !string.IsNullOrEmpty(s)));

        if (decision == "REJECT")
            return $"{companyName} için {result.TotalImpacts} sistem etkisi tespit edildi ({severity}). Acil eylem planı gerekli.";
        if (decision == "REVIEW")
            return $"{companyName} için {result.TotalImpacts} sistem etkisi tespit edildi ({severity}). Detaylı değerlendirme ve hazırlık öneriliyor.";
        return $"{companyName} için {result.TotalImpacts} sistem etkisi düşük seviyede — standart izleme yeterli.";
    }

    private static string MapDecisionToUrgency(string decision) => decision switch
    {
        "REJECT" => "CRITICAL",
        "REVIEW" => "HIGH",
        _ => "MEDIUM"
    };
}
