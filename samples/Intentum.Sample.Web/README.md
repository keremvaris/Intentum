# Intentum.Sample.Web

ASP.NET Core örnek uygulaması: Intent çıkarımı, açıklanabilirlik (explain), greenwashing tespiti, Dashboard ve analytics.

## Çalıştırma

```bash
dotnet run --project samples/Intentum.Sample.Web
```

- **UI:** http://localhost:5150/
- **API dokümanları (Scalar):** http://localhost:5150/scalar

## Özellikler

- **Intent infer / explain:** `POST /api/intent/infer`, `POST /api/intent/explain` — olaylardan niyet çıkarımı ve sinyal katkıları
- **Greenwashing tespiti:** `POST /api/greenwashing/analyze`, `GET /api/greenwashing/recent` — rapor analizi, çok dilli pattern’ler, opsiyonel görsel, Scope 3 / blockchain (mock)
- **Dashboard:** Analytics özeti, son çıkarımlar, son greenwashing analizleri (otomatik yenileme)
- **CQRS:** Carbon footprint (`/api/carbon/calculate`, `/api/carbon/report/{id}`), Orders (`POST /api/orders`)
- **Analytics:** `GET /api/intent/analytics/summary`, `GET /api/intent/history`, `/api/intent/analytics/export/json`, `/api/intent/analytics/export/csv`
- **Health:** `/health`

Detaylı API listesi için [API Referansı (EN)](../../docs/en/api.md#sample-web-http-api-intentumsampleweb) ve [Kurulum (EN)](../../docs/en/setup.md). Greenwashing akışı için [Greenwashing detection (how-to)](../../docs/en/greenwashing-detection-howto.md#6-sample-application-intentumsampleweb).
