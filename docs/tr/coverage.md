# Coverage (TR)

Intentum, test projesi için **kod coverage** üretir; böylece birim ve contract testlerinin hangi kod yollarını çalıştırdığını görebilirsin. Proje, kütüphane kodu için **en az %80** satır coverage hedefler; eşik test projesindeki Coverlet ayarı ve SonarCloud kalite kapısı ile zorunludur.

Bu sayfa coverage'ı yerelde nasıl üreteceğini, CI'da nasıl üretildiğini ve raporu nerede görüntüleyeceğini anlatır. Neyin test edildiği için [Test](testing.md).

---

## Mevcut durum

- **Hedef: %80+** satır coverage (kütüphane kodu). Test projesinde `Threshold=80` (Coverlet) tanımlı; SonarCloud kalite kapısı "Coverage on New Code" için aynı eşiği isteyebilir.
- **Kapsanan:** Çekirdek kütüphaneler (Intentum.Core, Intentum.Runtime, Intentum.AI), sağlayıcı IntentModel'leri (OpenAI, Gemini, Mistral, Azure) mock embedding sağlayıcı ile, policy ve clustering, options doğrulama, mock HTTP ile sağlayıcı parse.
- **CI:** `ci.yml` testleri `--collect:"XPlat Code Coverage;Format=opencover"` ile çalıştırır, raporu SonarCloud için yükler. `pages.yml` isteğe bağlı HTML raporu GitHub Pages'e yayımlayabilir.

---

## Yerelde coverage üretme

Reponun kökünden coverage ile test çalıştır (SonarCloud ile uyumlu OpenCover formatı):

```bash
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj \
  --collect:"XPlat Code Coverage;Format=opencover" \
  --results-directory TestResults
```

Coverage dosyası `TestResults/<run-id>/coverage.opencover.xml` altında yazılır.

İsteğe bağlı: ReportGenerator ile HTML rapor üret:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:TestResults/**/coverage.opencover.xml -targetdir:coverage -reporttypes:Html
```

Sonra tarayıcıda `coverage/index.html` aç.

---

## Son raporu görüntüleme (CI)

- **README badge:** README'deki coverage rozeti **SonarCloud**'dan gelir (her CI analizinden sonra güncellenir). Tıklayınca SonarCloud proje özeti açılır.
- **GitHub Pages:** `pages.yml` çalıştıysa HTML rapor `https://<org>.github.io/Intentum/coverage/index.html` adresinde olabilir (path workflow'a göre değişir).

---

## Notlar

- **SonarCloud hariç tutmalar:** CodeGen (CLI aracı), `*ServiceCollectionExtensions`, `*CachingExtensions`, `MultiTenancyExtensions` ve opsiyonel sağlayıcı (Claude) SonarCloud coverage'dan çıkarılmıştır; "Coverage on New Code" sadece test edilen kütüphaneyi yansıtır. Bkz. `.sonarcloud.properties`.
- **Eşikler:** Test projesinde (`Intentum.Tests.csproj`) `Threshold=80` ve `ThresholdType=line` ayarlı; SonarCloud kalite kapısı da yeni kod için %80 isteyebilir.
- **Hariç tutma:** Coverlet ile tip/metot hariç tutmak (örn. üretilen kod) için proje dosyasında Coverlet exclude seçenekleri veya attribute kullan.

Test yapısı ve kapsananlar için [Test](testing.md).
