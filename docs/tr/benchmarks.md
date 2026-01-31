# Benchmark'lar

Intentum, çekirdek işlemler için **BenchmarkDotNet** benchmark'ları içerir: davranış uzayından vektöre, intent inference (mock embedding ile) ve politika kararı. Makinenizde gecikme, throughput ve bellek ölçmek ve üretim boyutlandırması için performans belgelemek için kullanın.

---

## Neyi ölçüyoruz

| Benchmark | Ölçülen |
|-----------|---------|
| **ToVector_10Events** / **ToVector_1KEvents** / **ToVector_10KEvents** | `BehaviorSpace.ToVector()` — 10, 1K veya 10K olaydan davranış vektörünü oluşturma. |
| **LlmIntentModel_Infer_10Events** / **LlmIntentModel_Infer_1KEvents** | `LlmIntentModel.Infer(space)` — **Mock** embedding sağlayıcı ile tam inference (API çağrısı yok). |
| **PolicyEngine_Decide** | `intent.Decide(policy)` — politika değerlendirmesi (üç kural). |

Tüm benchmark'lar **Mock** embedding sağlayıcı kullanır; çalıştırmak için API anahtarı gerekmez ve sonuçlar yerelde tekrarlanabilir.

---

## Benchmark'ları çalıştırma

Reponun kökünden:

```bash
dotnet run --project benchmarks/Intentum.Benchmarks/Intentum.Benchmarks.csproj -c Release
```

Sonuçlar (Mean, Error, StdDev, Allocated) konsola yazılır. BenchmarkDotNet ayrıca `BenchmarkDotNet.Artifacts/results/` altına çıktı üretir:

- **\*-report.html** — tam rapor (tarayıcıda açın).
- **\*-report-github.md** — doküman veya README için uygun Markdown tablo.
- **\*-report.csv** — ileri analiz için ham sayılar.

### Tek bir benchmark filtreleme

```bash
dotnet run --project benchmarks/Intentum.Benchmarks/Intentum.Benchmarks.csproj -c Release -- --filter "*ToVector_10Events*"
```

---

## Dokümanlardaki benchmark sonuçlarını güncelleme

Dokümanlarda gösterilen benchmark sonuçlarını (örn. [Case studies — Benchmark sonuçları](../case-studies/benchmark-results.md)) güncellemek için:

```bash
./scripts/run-benchmarks.sh
```

Bu komut Release modunda benchmark'ları çalıştırır ve üretilen `*-report-github.md` dosyasını `docs/case-studies/benchmark-results.md` olarak kopyalar. Yayımlanan sonuçları kod tabanıyla senkron tutmak için bu dosyayı commit edebilirsiniz (isteğe bağlı; CI varsayılan olarak benchmark çalıştırmaz).

---

## Özet

| Madde | Nerede |
|-------|--------|
| Proje | [benchmarks/Intentum.Benchmarks](../../benchmarks/) |
| Çalıştırma | `dotnet run --project benchmarks/Intentum.Benchmarks/Intentum.Benchmarks.csproj -c Release` |
| Çıktılar | `BenchmarkDotNet.Artifacts/results/` |
| Doküman güncelleme | `./scripts/run-benchmarks.sh` → `docs/case-studies/benchmark-results.md` |

Sample.Web API için yük testi (örn. eşzamanlı infer endpoint) için [Yük testi: infer endpoint](../case-studies/load-test-infer.md) sayfasına bakın.

---

## İyileştirme fırsatları ve önerilen çözümler

Benchmark sonuçlarına göre (LlmIntentModel bellek ve gecikme event/boyut sayısıyla artıyor; PolicyEngine zaten hızlı) **somut adımlar**:

| Amaç | Çözüm |
|------|--------|
| **Büyük event setlerinde LlmIntentModel işini azaltmak** | `space.ToVector(new ToVectorOptions(CapPerDimension: N))` çağırıp sonucu geçir: `model.Infer(space, vector)`. Böylece benzersiz boyutlar sınırlanır, embedding çağrıları azalır. Veya `Infer(space, toVectorOptions)` extension’ını kullanın; bkz. [Gelişmiş Özellikler](advanced-features.md). |
| **Bellek ve API maliyetini düşürmek** | **CachedEmbeddingProvider** (veya Redis) kullanın; tekrarlayan behavior key’ler API’yi tekrar çağırmaz; daha az allocation ve gecikme. |
| **Production’da inference gecikmesini düşük tutmak** | **ChainedIntentModel** (önce kural tabanlı, LLM yedek) ile yüksek güvenli yollar LLM’e girmeden çözülsün; ToVectorOptions ile boyutları cap’leyin; aynı space birden çok değerlendiriliyorsa vektörü önceden hesaplayıp tekrar kullanın. |
| **Production’da daha büyük veri setleri** | Gerçekçi payload boyutlarıyla yük testi (örn. [Yük testi: infer endpoint](../case-studies/load-test-infer.md)); p95 artıyorsa boyut cap’i veya cache ekleyin. |
| **PolicyEngine** | Değişiklik gerekmez; zaten onlarca nanosaniye seviyesinde. |
