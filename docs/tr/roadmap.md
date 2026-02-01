# Yol haritası

**Bu sayfayı neden okuyorsunuz?** Bu sayfa Intentum'un yol haritasını özetler: v1.0 kriterleri, sonraki adımlar ve uzun vadeli hedefler. Proje yönünü veya katkı alanlarını merak ediyorsanız doğru yerdesiniz.

Intentum'un yönü: önce benimsenme, sonra derinlik.

---

## v1.0 kriterleri

- Çekirdek akış: Observe → Infer → Decide (BehaviorSpace, IIntentModel, IntentPolicy).
- Paketler: Core, Runtime, AI, AI sağlayıcıları (OpenAI, Gemini, Claude, Mistral, Azure), Testing, AspNetCore, Observability, Logging, Persistence, Analytics.
- Dokümantasyon: Neden Intentum, Manifesto, Canon, Gerçek dünya senaryoları, Niyet modelleri tasarlama, Neden Niyet ≠ Log.
- Örnekler: fraud-intent, ai-fallback-intent.
- CI: build, test, coverage, SonarCloud.

---

## v1.0 sonrası: benimsenme (A)

- "5 dakikada başla" — README'de.
- 2–3 gerçek use-case — fraud, AI fallback (docs ve examples).
- Minimal şablon repo — dotnet new / CodeGen.
- Topluluk: HN / Reddit / X paylaşımı hazır olunca.

---

## v1.0 sonrası: derinlik (B)

- Niyet skorlama stratejileri ve confidence kalibrasyonu.
- Daha zengin AI adapter'ları ve hibrit modeller.
- Akademik seviye dokümanlar.

---

## Son eklemeler (v1.0 sonrası)

Uygulanmış ve dokümante edilmiş:

- **Intent Timeline** — Entity-scoped intent geçmişi; `GetIntentTimelineAsync`, Sample: `GET /api/intent/analytics/timeline/{entityId}`.
- **Intent Tree** — Karar ağacı açıklanabilirliği; `IIntentTreeExplainer`, Sample: `POST /api/intent/explain-tree`.
- **Context-Aware Policy** — Context’li policy kuralları (load, region, recent intents); `ContextAwarePolicyEngine`, `intent.Decide(context, policy)`.
- **Policy Store** — JSON’dan deklaratif policy, hot-reload; `IPolicyStore`, `FilePolicyStore` (Intentum.Runtime.PolicyStore).
- **Behavior Pattern Detector** — Intent geçmişinde pattern ve anomali; `IBehaviorPatternDetector`.
- **Multi-Stage Intent Model** — Eşiklerle model zinciri; `MultiStageIntentModel`.
- **Scenario Runner** — Senaryoları model + policy ile çalıştırma; `IScenarioRunner`, `IntentScenarioRunner`.
- **Gerçek zamanlı stream** — `IBehaviorStreamConsumer`, `MemoryBehaviorStreamConsumer`; Worker şablonu kullanır.
- **OpenTelemetry tracing** — infer ve policy.evaluate span’leri; `IntentumActivitySource`.
- **Playground** — `POST /api/intent/playground/compare` ile model karşılaştırma.
- **dotnet new şablonları** — `intentum-webapi`, `intentum-backgroundservice`, `intentum-function`.

Bkz. [Gelişmiş Özellikler](advanced-features.md), [Kurulum – Şablondan oluştur](setup.md#şablondan-oluştur-dotnet-new) ve [API Referansı](api.md).

**Sonraki adım:** Bu sayfayı bitirdiyseniz → [Gelişmiş özellikler](advanced-features.md) veya [Kurulum](setup.md).

---
