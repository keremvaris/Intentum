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

## Render’a deploy (ücretsiz tier)

Uygulama Docker ile paketlenir; Render’da **native .NET** yok, **Docker** kullanılır.

1. [Render](https://render.com) → New → **Web Service**
2. Repo’yu bağla (GitHub/GitLab), branch seç
3. **Root Directory:** boş bırak (repo kökü)
4. **Environment:** **Docker**
5. **Dockerfile Path:** `samples/Intentum.Sample.Blazor/Dockerfile`
6. **Instance Type:** Free
7. Deploy’a tıkla

Render `PORT` env değişkenini verir; uygulama `Program.cs` içinde buna göre `0.0.0.0:PORT` üzerinde dinler. Health check için `/health` kullanılabilir.

- Free tier’da servis 15 dk trafik yoksa kapanır; ilk istekte ~1 dk cold start olur.
- Veritabanı in-memory olduğu için restart’ta veri sıfırlanır.
