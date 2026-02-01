# CodeGen (TR)

**Bu sayfayı neden okuyorsunuz?** Bu sayfa Intentum CodeGen ile CQRS + Intentum proje iskeleti oluşturmayı ve test assembly veya YAML spec'ten feature (Commands, Queries, Handlers) üretmeyi anlatır. Yeni proje scaffold veya kod üretimi arıyorsanız doğru yerdesiniz.

Intentum CodeGen, CQRS + Intentum proje iskeleti oluşturur ve test assembly veya YAML/JSON spec’ten **Features** (Commands, Queries, Handlers, Validators) üretir. **Herhangi** bir solution’da kullanılabilir: yeni proje veya mevcut Web API.

---

## Hızlı başlangıç (tam kullanım)

1. **Scaffold** ile CQRS + Intentum projesi oluştur (yeni veya mevcut klasöre):
   ```bash
   dotnet run --project src/Intentum.CodeGen/Intentum.CodeGen.csproj -- scaffold -o ./MyApp
   cd MyApp && dotnet build
   ```
2. **İsteğe bağlı — feature üret:** Test assembly veya YAML spec’ten:
   ```bash
   dotnet run --project src/Intentum.CodeGen/Intentum.CodeGen.csproj -- generate -a ./tests/MyApp.Tests/bin/Debug/net10.0/MyApp.Tests.dll -o ./MyApp
   # veya: -- generate -s features.yaml -o ./MyApp
   ```
3. **Çalıştır:** Uygulama varsayılan portta (`Properties/launchSettings.json`, örn. 5000) açılır. **UI:** `http://localhost:5000/`, **API dokümanı:** `http://localhost:5000/scalar`.

Mevcut dosyalar üzerine yazılmaz; CodeGen yalnızca eksik dosyaları ekler.

---

## Modlar

| Mod | Girdi | Çıktı |
|-----|--------|--------|
| **Scaffold** | Hedef klasör (`-o`) | Intentum + CQRS proje iskeleti, `Features/` klasörü, örnek feature |
| **Generate** | Test assembly (`-a`) veya spec dosyası (`-s`) + çıktı klasörü | `Features/<FeatureName>/` altında Commands, Queries, Handlers, Validators |

---

## CodeGen çalıştırma

Repo kökünden:

```bash
# Yeni veya mevcut bir klasöre proje iskeleti
dotnet run --project src/Intentum.CodeGen/Intentum.CodeGen.csproj -- scaffold -o ./MyApp
dotnet run --project src/Intentum.CodeGen/Intentum.CodeGen.csproj -- scaffold --output ./MyApp

# Test assembly'den (xUnit [Fact] metodları) CQRS feature kodu üret
dotnet run --project src/Intentum.CodeGen/Intentum.CodeGen.csproj -- generate -a ./tests/MyApp.Tests/bin/Debug/net10.0/MyApp.Tests.dll -o ./src/MyApp.Web

# YAML spec'ten üret
dotnet run --project src/Intentum.CodeGen/Intentum.CodeGen.csproj -- generate -s features.yaml -o ./src/MyApp.Web
```

**Herhangi bir projede** kullanılabilir: `-o` ile mevcut Web API veya class library’yi hedefleyin; CodeGen dosyaları o klasöre yazar.

---

## dotnet new şablonu

Intentum CQRS şablonunu kurup yeni proje oluşturma:

```bash
dotnet new install ./templates/intentum-cqrs
dotnet new intentum-cqrs -n MyApp -o ./MyApp
```

---

## Test assembly → Features

CodeGen, test assembly içinde **xUnit `[Fact]`** (veya benzeri) attribute’lu metodları tarar ve **metod adını** convention’a göre ayrıştırır:

- Kalıp: `FeatureName_Aksiyon_ByX` (örn. `CarbonFootprintCalculation_AllowsOrObserves_ByConfidence`).
- **Feature adı** = ilk parça (`CarbonFootprintCalculation`).
- Her benzersiz feature adı için üretir:
  - `Features/<FeatureName>/Commands/` — bir command + result record.
  - `Features/<FeatureName>/Handlers/` — command handler.
  - `Features/<FeatureName>/Validators/` — FluentValidation validator (isteğe bağlı).

Handler ve validator’lar minimal stub’dır; [Sample.Blazor](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Blazor) örneğindeki gibi handler içinde Intentum (Observe → Infer → Decide) ekleyebilirsiniz.

---

## YAML spec formatı

Örnek `features.yaml`:

```yaml
namespace: MyApp.Web
features:
  - name: CarbonFootprintCalculation
    commands:
      - name: CalculateCarbon
        properties:
          - name: Actor
            type: string
          - name: Scope
            type: string
    queries:
      - name: GetCarbonReport
        properties:
          - name: ReportId
            type: string
```

- **namespace** — Üretilen C# için kök namespace (varsayılan: `Intentum.Cqrs.Web`).
- **features** — Feature listesi; her biri **commands** ve **queries** içerebilir.
- **commands** / **queries** — Name ve isteğe bağlı **properties** (name + type). Üretilen kod MediatR `IRequest<T>` ve uygun yerlerde FluentValidation kullanır.

---

## Çıktı yapısı

Scaffold ve generate, [Sample.Blazor](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Blazor) ile uyumlu bu yapıyı hedefler:

```
Features/
  <FeatureName>/
    Commands/
    Queries/
    Handlers/
    Validators/
```

Mevcut dosyalar **üzerine yazılmaz**; CodeGen yalnızca eksik dosyaları yazar.

---

## Sample.Blazor kısaca

[Sample.Blazor](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Blazor) projesi, CodeGen çıktısı ve handler içinde Intentum için referanstır:

- **Stack:** ASP.NET Core, MediatR, FluentValidation, Intentum (Core, Runtime, AI), **Scalar** (API dokümanı), `wwwroot/` içinde statik **UI**.
- **Endpoint’ler:** `POST /api/carbon/calculate` (Intentum Observe→Infer→Decide), `GET /api/carbon/report/{id}`, `POST /api/orders`.
- **UI:** `http://localhost:5000/` — Carbon, Rapor ve Sipariş formları. **API dokümanı:** `http://localhost:5000/scalar`.
- **Port:** `Properties/launchSettings.json` içinde varsayılan 5000; gerekirse değiştirilebilir.
- **Çalıştırma:** `dotnet run --project samples/Intentum.Sample.Blazor`.

---

## Ayrıca bakınız

- [Kurulum](setup.md) — Intentum paketlerini yükleme.
- [Senaryolar](scenarios.md) — Observe → Infer → Decide örnekleri.
- [Sample.Blazor](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample.Blazor) — Tam CQRS + Intentum ASP.NET örneği (Scalar + UI).

**Sonraki adım:** Bu sayfayı bitirdiyseniz → [Kurulum](setup.md) veya [Senaryolar](scenarios.md).
