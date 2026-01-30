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

## SonarCloud: bulgular ve kalite kapısı

- **Sonuçları nerede görürsünüz:** CI çalıştıktan sonra [SonarCloud](https://sonarcloud.io) açıp Intentum projesini seçin. README rozetleri (Coverage, SonarCloud alert status) proje özetine gider.
- **Kalite kapısı:** SonarCloud "Coverage on New Code", "Duplications", "Maintainability", "Reliability", "Security" değerlendirir. **Alert status** rozeti kalite kapısı geçtiğinde yeşil olur. Kapının yeşil kalması için yeni bulguları (bug, güvenlik açığı, code smell) giderin.
- **Coverage on New Code:** Kalite kapısında yalnızca *yeni* kodun %80 satır coverage hedefini karşılaması istenir. Mevcut kod raporlanır ama kapıyı düşürmez. Hariç tutulan yollar (aşağıya bakın) sayılmaz.
- **Bulguları bulma ve giderme:** SonarCloud'da "Issues" ile bug, güvenlik açığı ve code smell'leri görün. Merge öncesi yeni bulguları giderin; rehber için "Why is this an issue?" kullanın. Sık düzeltmeler: async için `await` kullanın, gereksiz koşullardan kaçının, tekrarlayan sabitler için sabit tercih edin, gerekli yerde null kontrolü ekleyin.

---

## Notlar

- **SonarCloud hariç tutmalar:** CodeGen (CLI aracı), `*ServiceCollectionExtensions`, `*CachingExtensions`, `MultiTenancyExtensions` ve opsiyonel sağlayıcı (Claude) SonarCloud coverage'dan çıkarılmıştır; "Coverage on New Code" sadece test edilen kütüphaneyi yansıtır. Persistence adaptörleri (MongoDB, Redis) de hariç tutulabilir. Bkz. `.sonarcloud.properties` ve CI workflow `sonar.coverage.exclusions`.
- **Eşikler:** Test projesinde (`Intentum.Tests.csproj`) `Threshold=80` ve `ThresholdType=line` ayarlı; SonarCloud kalite kapısı da yeni kod için %80 isteyebilir.
- **Hariç tutma:** Coverlet ile tip/metot hariç tutmak (örn. üretilen kod) için proje dosyasında Coverlet exclude seçenekleri veya attribute kullan.

Test yapısı ve kapsananlar için [Test](testing.md).
