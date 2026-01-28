# Coverage (TR)

Intentum, test projesi için **kod coverage** üretir; böylece birim ve contract testlerinin hangi kod yollarını çalıştırdığını görebilirsin. Coverage %100 değil; odak çekirdek davranış ve sağlayıcı contract’ları üzerinde.

Bu sayfa coverage’ı yerelde nasıl üreteceğini, CI’da nasıl üretildiğini ve raporu nerede görüntüleyeceğini anlatır. Neyin test edildiği için [Test](testing.md).

---

## Mevcut durum

- **Coverage %100 değil.** Proje contract testleri ve ana davranış yollarına (BehaviorSpace, Infer, Decide, sağlayıcı parse) öncelik veriyor.
- **Kapsanan:** Çekirdek kütüphaneler (Intentum.Core, Intentum.Runtime, Intentum.AI) ve mock HTTP ile sağlayıcı parse. Bazı kenar yollar veya opsiyonel özellikler kapsanmamış olabilir.
- **CI:** GitHub Actions workflow’u (örn. `pages.yml` veya `ci.yml`) testleri coverage ile çalıştırıp HTML raporunu GitHub Pages’e (örn. `/coverage/index.html`) yayımlayabilir.

---

## Yerelde coverage üretme

Reponun kökünden Coverlet ve OpenCover formatıyla test çalıştır:

```bash
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=TestResults/coverage/
```

İsteğe bağlı: satır satır coverage görmek için ReportGenerator ile HTML rapor üret:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:TestResults/coverage/coverage.opencover.xml -targetdir:coverage -reporttypes:Html
```

Sonra tarayıcıda `coverage/index.html` aç.

---

## Son raporu görüntüleme (CI)

Workflow’un coverage’ı GitHub Pages’e yayımlıyorsa:

- **HTML rapor:** `https://<org>.github.io/Intentum/coverage/index.html` (veya Pages workflow’unda tanımlı path).
- **Badge’ler:** Bazı workflow’lar README’ye coverage badge’i (örn. satır coverage %) ekler; badge rapora gider.

Tam path ve artifact yapısı için `pages.yml` (veya benzeri) dosyana bak.

---

## Notlar

- **ReportGenerator** yerel kullanım için opsiyonel; CI zaten yayımlanan HTML ve badge’ler için kullanıyor olabilir.
- **Eşikler:** Test veya CI adımında coverage eşiği (örn. satır coverage X%’in altına düşerse build fail) ekleyebilirsin; bu repo varsayılan olarak zorunlu tutmuyor.
- **Hariç tutma:** Coverage’dan tip veya metot hariç tutmak (örn. üretilen kod) için Coverlet exclude seçenekleri veya proje dosyasında attribute kullan.

Test yapısı ve kapsananlar için [Test](testing.md).
