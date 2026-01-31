# Testler Özeti

Bu sayfa Intentum testlerinin kısa bir özetini verir: nelerin kapsandığı, nasıl çalıştırılacağı ve **sample** / **örnekler**le ilişkisi.

---

## Test projeleri

| Proje | Açıklama |
|--------|----------|
| **Intentum.Tests** | Birim ve sözleşme testleri: BehaviorSpace, çıkarım, politika, sağlayıcılar (mock HTTP), clustering, açıklanabilirlik, simülasyon, versiyonlama, çok kiracılık, olaylar, deneyler. Gerçek API anahtarı yok. |
| **Intentum.Tests.Integration** | Entegrasyon testleri: greenwashing örnek olay (etiketli veri üzerinde accuracy/F1). İsteğe bağlı: env var ile gerçek API’ler. |

---

## Nasıl çalıştırılır

Reponun kökünden:

```bash
# Tüm birim testler (API anahtarı gerekmez)
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj

# Anahtar yokken sağlayıcı entegrasyon testlerini hariç tut
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName!=Intentum.Tests.OpenAIIntegrationTests&FullyQualifiedName!=Intentum.Tests.AzureOpenAIIntegrationTests&FullyQualifiedName!=Intentum.Tests.GeminiIntegrationTests&FullyQualifiedName!=Intentum.Tests.MistralIntegrationTests"

# Entegrasyon testleri
dotnet test tests/Intentum.Tests.Integration/Intentum.Tests.Integration.csproj
```

Detay için [Test](testing.md), script’ler için [Yerel entegrasyon testleri](local-integration-tests.md).

---

## Neler kapsanıyor (özet)

- **Çekirdek:** BehaviorSpace, ToVector, niyet güveni, politika motoru (Evaluate, EvaluateWithRule), rate limit, yerelleştirme.
- **Modeller:** Kural tabanlı, chained, multi-stage, LLM (mock + mock HTTP ile sağlayıcı parsing).
- **Analitik:** IntentAnalytics (trendler, dağılım, anomaliler, timeline, export).
- **Açıklanabilirlik:** IntentExplainer, IntentTreeExplainer, karar ağacı.
- **Kalıcılık:** In-memory repo (history + behavior space); birim testler için EF/Redis/Mongo gerekmez.
- **Simülasyon:** BehaviorSpaceSimulator, ScenarioRunner.
- **Kalıplar:** BehaviorPatternDetector, şablon eşleme.
- **Politika store:** FilePolicyStore, SafeConditionBuilder (deklaratif kurallar).

Entegrasyon testleri greenwashing accuracy/F1’i etiketli veri üzerinde kapsar; bkz. [Örnek olay — Greenwashing metrikleri](../case-studies/greenwashing-metrics.md).

---

## Testler vs sample vs örnekler

| | Testler | Örnekler | Sample |
|--|--------|----------|--------|
| **Amaç** | Sözleşme ve çekirdek davranışı doğrulamak; CI’da gerçek API yok. | Tek kullanım senaryosunu öğrenmek; kopyala-yapıştır. | Tam uygulama: birçok özellik, Web API, UI. |
| **Çalıştırma** | `dotnet test tests/Intentum.Tests` | `dotnet run --project examples/<ad>` | `dotnet run --project samples/Intentum.Sample.Web` |
| **Döküman** | [Test](testing.md), bu sayfa | [Örnekler rehberi](examples-overview.md) | [API](api.md), [Kurulum](setup.md) |

---

## Test ekleme

- Gerçek API çağrısı olmaması için **mock HttpClient** veya in-memory sağlayıcı kullanın.
- Yeni özellikler (timeline, niyet ağacı, pattern dedektörü vb.) için `Intentum.Tests` içinde test ekleyin ve [Test](testing.md) içindeki “Neler kapsanıyor” listesini güncelleyin.
