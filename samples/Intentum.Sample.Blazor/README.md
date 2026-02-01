# Intentum.Sample.Blazor

Blazor örnek uygulaması: Intent çıkarımı, açıklanabilirlik (explain), greenwashing tespiti, Dashboard ve analytics.

## Çalıştırma

```bash
dotnet run --project samples/Intentum.Sample.Blazor
```

- **UI:** http://localhost:5018/
- **API dokümanları (Scalar):** http://localhost:5018/scalar

## Özellikler

- **Intent infer / explain:** `POST /api/intent/infer`, `POST /api/intent/explain` — olaylardan niyet çıkarımı ve sinyal katkıları
- **Greenwashing tespiti:** `POST /api/greenwashing/analyze`, `GET /api/greenwashing/recent` — rapor analizi, çok dilli pattern'ler, opsiyonel görsel, Scope 3 / blockchain (mock)
- **Dashboard:** Analytics özeti, son çıkarımlar, son greenwashing analizleri (otomatik yenileme)
- **CQRS:** Carbon footprint (`/api/carbon/calculate`, `/api/carbon/report/{id}`), Orders (`POST /api/orders`)
- **Analytics:** `GET /api/intent/analytics/summary`, `GET /api/intent/history`, `/api/intent/analytics/export/json`, `/api/intent/analytics/export/csv`
- **Health:** `/health`
- **Blazor sayfaları:** Overview, Commerce, Explain, FraudLive, Sustainability, Timeline, PolicyLab, Sandbox, Settings, Signals, Graph, Heatmap; SSE inference, dolandırıcılık ve sürdürülebilirlik simülasyonu

Detaylı API listesi için [API Referansı (EN)](../../docs/en/api.md#sample-blazor-http-api-intentumsampleblazor) ve [Kurulum (EN)](../../docs/en/setup.md).
