# Greenwashing tespiti (how-to)

Intentum ile sürdürülebilirlik raporlarında greenwashing tespiti: davranış uzayı, niyet çıkarımı ve politika kararları.

---

## Sorun (klasik yaklaşım)

Klasik sistemler şöyle yapar:

```
IF report.Contains("sustainable") AND !report.Contains("data") THEN flag_greenwashing
IF report.Contains("green") AND report.Contains("images") AND !report.Contains("metrics") THEN suspicious
```

Sorunlar:

- Kelime tabanlı kurallar ifade değişince kırılır
- İkili sonuç (şüpheli / değil) risk derecesini yok sayar
- Tespit edilen pattern’ler ile önerilen aksiyonlar arasında bağ yok
- Yeni greenwashing tekniklerine uyarlamak zor

---

## Intentum’un sorduğu soru

Şirket ne iletmeye çalışıyor? Gerçekten sürdürülebilir olma çabası mı, yoksa öyle görünme mi?

Cevap evet-hayır değil. Intentum, gözlemlenen davranış sinyallerinden niyet ve güven skoru çıkarır.

---

## 1. Rapor metninden davranış uzayı

Sürdürülebilirlik raporundan sinyalleri toplayıp davranış event’leri olarak kaydedin. `BehaviorSpace` ve `Observe(actor, action)` extension’ını kullanın.

**Actor** = sinyal kategorisi (örn. `"language"`, `"data"`, `"imagery"`).  
**Action** = somut sinyal (örn. `"claim.vague"`, `"metrics.without.proof"`).

Örnek: belirsiz iddialar, kanıtsız karşılaştırmalar, kanıtsız metrikler, uygun baz yılı seçimi.

```csharp
using System.Text.RegularExpressions;
using Intentum.Core;
using Intentum.Core.Behavior;

var space = new BehaviorSpace();
var regexTimeout = TimeSpan.FromSeconds(1);

// Belirsiz sürdürülebilirlik dili
foreach (var pattern in new[] { "sustainable future", "green transition", "eco-friendly", "clean production" })
{
    if (report.Contains(pattern))
    {
        var count = Regex.Matches(report, Regex.Escape(pattern), RegexOptions.IgnoreCase, regexTimeout).Count;
        for (int i = 0; i < count; i++)
            space.Observe("language", "claim.vague");
    }
}

// Kanıtsız karşılaştırmalı iddialar
if (HasComparativeClaims(report))
    space.Observe("language", "comparison.unsubstantiated");

// Doğrulama olmadan metrik (ISO, audit, verified)
var hasMetrics = Regex.IsMatch(report, @"%\s*(reduction|increase)|(\d+\s*(ton|kg|kWh|CO2))", RegexOptions.IgnoreCase, regexTimeout);
var hasProof = report.Contains("ISO") || report.Contains("verified") || report.Contains("audit");
if (hasMetrics && !hasProof)
    space.Observe("data", "metrics.without.proof");

// Uygun baz yılı seçimi
if (UsesFavorableBaseline(report))
    space.Observe("data", "baseline.manipulation");

// Veri olmadan doğa görseli
if (HasNatureImageryWithoutData(report))
    space.Observe("imagery", "nature.without.data");
```

Event’leri okumak için `space.Events`; toplam sinyal sayısı `space.Events.Count`. Event seviyesinde metadata (örn. pattern metni, sayı) için `BehaviorSpaceBuilder` ve `BehaviorEvent(actor, action, occurredAt, metadata)` kullanın.

---

## 2. Niyet modeli

Davranış uzayını isimli niyet ve güvene map’lemek için `IIntentModel` implemente edin. Seçenekler:

- **Kural tabanlı:** `behaviorSpace.Events` üzerinden sayım yapan kurallarla `RuleBasedIntentModel` kullanın; her kural `RuleMatch(Name, Score, Reasoning)` dönsün. Yerleşik model ilk eşleşen kuralı, davranış vektöründen türetilen `Signals` ile bir `Intent`’e çevirir.
- **Özel:** `IIntentModel.Infer(BehaviorSpace, BehaviorVector?)` kendiniz yazın: greenwashing sinyallerini toplayın, ağırlık/skor hesaplayın, niyet adını seçin (örn. ActiveGreenwashing, StrategicObfuscation, SelectiveDisclosure, UnintentionalMisrepresentation, GenuineSustainability) ve `Intent(Name, Signals, Confidence, Reasoning)` döndürün.

“Tespit edilen pattern’ler”i `Intent.Signals` içinde taşıyın: örn. `new IntentSignal("greenwashing", "language:claim.vague", weight)`. Kısa özet için `Reasoning` kullanın.

Örnek niyet kategorizasyonu (özel mantık):

- Ağırlık &gt; 0.8 → `ActiveGreenwashing`
- Ağırlık &gt; 0.6 → `StrategicObfuscation`
- Ağırlık &gt; 0.4 → `SelectiveDisclosure`
- Ağırlık &gt; 0.2 → `UnintentionalMisrepresentation`
- Diğer → `GenuineSustainability`

Güven: `IntentConfidence.FromScore(score)`; `Level` metni `"Low"`, `"Medium"`, `"High"` veya `"Certain"`.

---

## 3. Politika

Niyet adı ve güveni `PolicyDecision`’a map’lemek için `IntentPolicyBuilder` kullanın. Intentum "IMMEDIATE_REVIEW" gibi özel aksiyon tanımlamaz; enum’a map’leyip uygulamanızda yorumlarsınız.

Önerilen eşleme:

- **Kritik risk:** `ActiveGreenwashing` ve yüksek güven → `Block` veya `Escalate`
- **Doğrulama gerekli:** `StrategicObfuscation` veya orta+ güvende `SelectiveDisclosure` → `Warn` veya `RequireAuth`
- **İzleme:** Düşük risk ama sıfırdan büyük güven → `Observe`
- **Kabul edilebilir:** `GenuineSustainability` veya düşük güven → `Allow`

```csharp
using Intentum.Runtime;
using Intentum.Runtime.Policy;

var policy = new IntentPolicyBuilder()
    .Escalate("CriticalGreenwashing",
        i => i.Name == "ActiveGreenwashing" && i.Confidence.Score >= 0.7)
    .Warn("NeedsVerification",
        i => i.Name == "StrategicObfuscation" ||
             (i.Name == "SelectiveDisclosure" && i.Confidence.Score >= 0.5))
    .Observe("Monitor",
        i => i.Confidence.Score > 0.3)
    .Allow("LowRisk", _ => true)
    .Build();

var decision = intent.Decide(policy);
```

`decision` (örn. `Escalate`, `Warn`, `Observe`) ile `intent.Name` ve `intent.Signals`’ı uygulamanızda somut aksiyonlara (acil inceleme, üçüncü taraf denetim, çeyreklik izleme) map’leyin.

---

## 4. Çözüm katmanı (uygulama seviyesi)

Intentum çözüm DTO’ları sağlamaz. `Intent`, `BehaviorSpace` ve `PolicyDecision` alıp kendi “çözüm paketinizi” (acil aksiyonlar, iletişim düzeltmeleri, doğrulama adımları) üreten ince bir katman yazın. Mantığı `intent.Name`, `intent.Signals` ve `intent.Confidence` ile besleyin; ham sinyaller için `space.Events` kullanın.

Örnek yapı:

- `intent.Name == "ActiveGreenwashing"` ve `decision == PolicyDecision.Escalate` ise: acil aksiyonlar ekleyin (iddiaları askıya al, dahili inceleme, destekleyici veriyi yayımla).
- Sinyaller `"data:metrics.without.proof"` içeriyorsa: “tüm metrik iddiaları için destekleyici veriyi yayımla”.
- Sinyaller `"data:baseline.manipulation"` içeriyorsa: “sektör standardı baz yılı ile yeniden hesapla”.

---

## 5. Özet

| Yaklaşım | Intentum |
|----------|----------|
| Kelime / kural eşleme | Davranış uzayı + niyet çıkarımı |
| İkili (evet/hayır) | Güven skoru ve seviye |
| Sabit kurallar | Politika niyet + güven → karar |
| Kapalı kutu | Sinyaller ve reasoning niyeti açıklar |

Yeni sinyaller = yeni gözlemlerle esneklik, ikili bayrak yerine güven skoru ve `Intent.Signals` / `Reasoning` ile açıklanabilirlik elde edersiniz. Politika niyet modelinden ayrı kalır; kuralları yeniden eğitim olmadan değiştirebilirsiniz.

---

## Ayrıca bakınız

- [Gerçek dünya senaryoları](real-world-scenarios.md) — Dolandırıcılık/kötüye kullanım ve AI yedeklemesi
- [Niyet modelleri tasarlama](designing-intent-models.md) — Heuristic vs ağırlıklı vs LLM
- Örnek: [examples/greenwashing-intent](https://github.com/keremvaris/Intentum/tree/master/examples/greenwashing-intent) — Repodaki çalıştırılabilir örnek
