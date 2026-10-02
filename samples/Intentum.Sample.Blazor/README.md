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
- **System One motorları:** `POST /api/intent/systemone/infer?engine=laya` ve `/system-one` sayfası — Jev, Kev, Laya, TinyJev gibi `/v1/systemone` uyumlu motorlardan gerçek çıkarım (`SYSTEMONE_ENGINE`, `SYSTEMONE_BASE_URL`, `SYSTEMONE_API_KEY` / `TYPESAFE_API_KEY` ortam değişkenleri; bkz. [examples/system-one-decision](../../examples/system-one-decision/README.md))
- **Greenwashing tespiti:** `POST /api/greenwashing/analyze`, `GET /api/greenwashing/recent` — rapor analizi, çok dilli pattern'ler, opsiyonel görsel, Scope 3 / blockchain (mock)
- **Dashboard:** Analytics özeti, son çıkarımlar, son greenwashing analizleri (otomatik yenileme)
- **CQRS:** Carbon footprint (`/api/carbon/calculate`, `/api/carbon/report/{id}`), Orders (`POST /api/orders`)
- **Analytics:** `GET /api/intent/analytics/summary`, `GET /api/intent/history`, `/api/intent/analytics/export/json`, `/api/intent/analytics/export/csv`
- **Health:** `/health`
- **Blazor sayfaları:** Overview, Commerce, Explain, FraudLive, Sustainability, Timeline, PolicyLab, Sandbox, Settings, Signals, Graph, Heatmap; SSE inference, dolandırıcılık ve sürdürülebilirlik simülasyonu. **Gerçek vs demo:** Overview'da "Gerçek / Simülasyon" kartı; infer, politika ve analytics gerçek pipeline; olay üreticileri ve "Demo Başlat" ile üretilen veriler simülasyondur.

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

## System One demo (Render)

`/system-one` sayfası varsayılan olarak süreç içi `demo` motorunu kullanır (anahtarsız, harici servis gerekmez); yapılandırma gerekmez. Opsiyonel Render ortam değişkenleri:

- `SYSTEMONE_ENGINE` — sayfanın varsayılan motoru (`demo`, `kev`, `laya`, `jev`, ...)
- `SYSTEMONE_BASE_URL` — seçili motorun base URL değerini ezerek geçer
- `SYSTEMONE_API_KEY` — `jev` için varsayılan API anahtarı (UI alanındaki istek-anahtarını tercih edin)

Base URL alanı / `SYSTEMONE_BASE_URL`, sunucunun istediğiniz bir host'a istek atmasını sağlar — yalnızca güvendiğiniz motorlara yönlendirin.
