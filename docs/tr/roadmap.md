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

## Son eklemeler (v1.1 - v1.2)

Uygulanmış ve dokümante edilmiş:

### v1.1 — Önceki eklemeler
- **Intent Timeline** — Entity-scoped intent geçmişi; `GetIntentTimelineAsync`, Sample: `GET /api/intent/analytics/timeline/{entityId}`.
- **Intent Tree** — Karar ağacı açıklanabilirliği; `IIntentTreeExplainer`, Sample: `POST /api/intent/explain-tree`.
- **Context-Aware Policy** — Context'li policy kuralları (load, region, recent intents); `ContextAwarePolicyEngine`, `intent.Decide(context, policy)`.
- **Policy Store** — JSON'dan deklaratif policy, hot-reload; `IPolicyStore`, `FilePolicyStore` (Intentum.Runtime.PolicyStore).
- **Behavior Pattern Detector** — Intent geçmişinde pattern ve anomali; `IBehaviorPatternDetector`.
- **Multi-Stage Intent Model** — Eşiklerle model zinciri; `MultiStageIntentModel`.
- **Scenario Runner** — Senaryoları model + policy ile çalıştırma; `IScenarioRunner`, `IntentScenarioRunner`.
- **Gerçek zamanlı stream** — `IBehaviorStreamConsumer`, `MemoryBehaviorStreamConsumer`; Worker şablonu kullanır.
- **OpenTelemetry tracing** — infer ve policy.evaluate span'leri; `IntentumActivitySource`.
- **Playground** — `POST /api/intent/playground/compare` ile model karşılaştırma.
- **dotnet new şablonları** — `intentum-webapi`, `intentum-backgroundservice`, `intentum-function`.

### v1.2 — Yeni eklemeler
- **Resilience Pattern'leri** — Circuit Breaker, Retry, Bulkhead, Degradation, Timeout (in-memory); `ICircuitBreaker`, `IRetryPolicy`, `IBulkhead`, `IDegradationPolicy`, `ITimeoutPolicy` (Intentum.Runtime.Resilience).
- **Domain Modules** — 5 domain: Healthcare, Education, IoT, Finance, Supply Chain (8'er kural); `HealthcareRules`, `EducationRules`, `IoTRules`, `FinanceRules`, `SupplyChainRules` (Intentum.Core).
- **CLI Tool** — `intentum` global tool: scaffold model/policy, validate, test-infer, export openapi; `Intentum.Cli`.
- **Blazor Playground** — Interaktif web playground: BehaviorSpace editor, Policy editor, Inference demo; `Intentum.Playground`.
- **OpenAPI Specification** — REST API için OpenAPI 3.0 spec; `docs/openapi/intentum.yaml`.
- **SDK Generation** — C#, Python, TypeScript SDK generation scripts; `sdk/`.
- **VS Code Extension** — Package Explorer ve kod snippet'leri; `extensions/vscode-intentum/`.
- **Confidence Calibration** — Platt scaling ve Temperature scaling; `IConfidenceCalibrator`, `PlattCalibrator`, `TemperatureCalibrator` (Intentum.AI.Calibration).
- **Few-Shot Learning** — Örnek tabanlı intent tanıma; `IFewShotStore`, `MemoryFewShotStore`, `FewShotIntentModel` (Intentum.AI.FewShot).
- **Multi-Modal Fusion** — Birden fazla modality'yi birleştirme; `MultiModalFusion`, `MultiModalIntentModel` (Intentum.AI.MultiModal).
- **Ensemble Models** — Weighted average ve Majority voting; `IEnsembleStrategy`, `WeightedEnsemble`, `MajorityVotingEnsemble` (Intentum.AI.Ensemble).
- **Token Cost Tracking** — Token sayma ve maliyet takibi; `ITokenCounter`, `ITokenCostTracker`, `MemoryTokenCostTracker` (Intentum.AI.TokenCost).
- **Distributed Locking** — Redis tabanlı dağıtık kilit; `IDistributedLock`, `RedisDistributedLock` (Intentum.Distributed).
- **Distributed Rate Limiter** — Redis tabanlı dağıtık rate limiter; `IDistributedRateLimiter`, `RedisDistributedRateLimiter` (Intentum.Distributed).
- **Event Sourcing Interfaces** — Domain event, aggregate root, event store, event bus; `IDomainEvent`, `IAggregateRoot`, `IEventStore`, `IEventBus` (Intentum.Distributed).
- **Outbox Pattern** — Transactional outbox interfaces; `IOutboxStore`, `IOutboxProcessor` (Intentum.Distributed).
- **gRPC Service** — Infer ve Evaluate RPC'leri; `IntentumGrpcService` (Intentum.Grpc).
- **DeepSeek AI Provider** — DeepSeek embedding + intent modeli; `DeepSeekEmbeddingProvider`, `DeepSeekIntentModel` (Intentum.AI.DeepSeek).
- **MCP Server** — AI agent'lar için Model Context Protocol sunucusu; `POST /mcp/infer`, `POST /mcp/evaluate` (Intentum.McpServer).

Bkz. [Gelişmiş Özellikler](advanced-features.md), [Üretim Hazırlığı](production-readiness.md), [Kurulum – Şablondan oluştur](setup.md#şablondan-oluştur-dotnet-new) ve [API Referansı](api.md).

**Sonraki adım:** Bu sayfayı bitirdiyseniz → [Gelişmiş özellikler](advanced-features.md) veya [Kurulum](setup.md).

---
