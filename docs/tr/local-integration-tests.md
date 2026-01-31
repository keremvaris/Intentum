# Yerel entegrasyon testleri ve VerifyAI

Bu sayfa **gerçek API entegrasyon testlerini** (OpenAI, Mistral, Gemini, Azure OpenAI) ve **VerifyAI** uygulamasını yerelde nasıl çalıştıracağınızı anlatır. API anahtarı gerektirir; CI'da varsayılan olarak **çalıştırılmaz** (CI `Category=Integration` hariç tutar); yalnızca kendi makinenizde kullanın.

---

## VerifyAI (tek uygulama: tüm sağlayıcılar, embedding + tam pipeline)

**VerifyAI**, anahtar tanımladığınız her sağlayıcı için Intentum’un çalıştığını doğrulayan tek giriş noktasıdır. Her sağlayıcı için **embedding** ve **tam pipeline** (BehaviorSpace → Infer → Policy) çalıştırır.

### Tek seferlik kurulum

1. Şablonu reponun kökünde `.env` olarak kopyalayın:
   ```bash
   cp .env.example .env
   ```
2. `.env` dosyasını düzenleyip en az bir sağlayıcının anahtarını yazın (değişken adları için `.env.example`’a bakın):
   - **OpenAI:** `OPENAI_API_KEY`, isteğe bağlı `OPENAI_BASE_URL`, `OPENAI_EMBEDDING_MODEL`
   - **Mistral:** `MISTRAL_API_KEY`, isteğe bağlı `MISTRAL_BASE_URL`, `MISTRAL_EMBEDDING_MODEL`
   - **Gemini:** `GEMINI_API_KEY`, isteğe bağlı `GEMINI_BASE_URL`, `GEMINI_EMBEDDING_MODEL`
   - **Azure OpenAI:** `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY`, isteğe bağlı `AZURE_OPENAI_EMBEDDING_DEPLOYMENT`, `AZURE_OPENAI_API_VERSION`

### VerifyAI çalıştırma

Reponun kökünden:

```bash
dotnet run --project samples/Intentum.VerifyAI
```

Uygulama `.env`’i yükler; anahtarı olan her sağlayıcı için: (1) embedding API’yi çağırır, (2) tam intent pipeline’ını (Infer + Policy) çalıştırır ve `[OK] SağlayıcıAdı` veya `[FAIL]` / `[SKIP]` ile mesaj yazar.

### Sadece belirli sağlayıcıları çalıştırma

Gereksiz istek atmamak için yalnızca seçtiğiniz sağlayıcıları çalıştırabilirsiniz. Ortam değişkeni: `VERIFY_AI_PROVIDERS`, virgülle ayrılmış: `OpenAI`, `Azure`, `Gemini`, `Mistral`.

Örnek — sadece Mistral:

```bash
VERIFY_AI_PROVIDERS=Mistral dotnet run --project samples/Intentum.VerifyAI
```

Örnek — Mistral ve OpenAI:

```bash
VERIFY_AI_PROVIDERS=Mistral,OpenAI dotnet run --project samples/Intentum.VerifyAI
```

Belirtilmezse `.env`'de anahtarı olan **tüm** sağlayıcılar çalıştırılır.

### Verbose: istek/yanıt gövdesini görme

HTTP istek ve yanıt gövdesini (örn. hata ayıklama için) görmek için `VERIFY_AI_VERBOSE=1` kullanın. Sadece bir sağlayıcının gelen/giden datasını görmek için `VERIFY_AI_PROVIDERS` ile birlikte kullanın:

```bash
VERIFY_AI_PROVIDERS=Mistral VERIFY_AI_VERBOSE=1 dotnet run --project samples/Intentum.VerifyAI
```

Tüm sağlayıcılar için verbose:

```bash
VERIFY_AI_VERBOSE=1 dotnet run --project samples/Intentum.VerifyAI
```

Veya `.env`’e `VERIFY_AI_VERBOSE=1` ekleyebilirsiniz.

---

## Entegrasyon testleri (xUnit, sağlayıcı bazlı)

Entegrasyon test sınıfları, ilgili env değişkenleri set edildiğinde gerçek API’yi çağırır. Anahtar yoksa **açık mesajla fail** eder (sessiz atlama yok). İndirilen veri gerektiren testler (örn. Mendeley Excel veya HTML için GreenwashingCaseStudyTests) de `Category=Integration` kullanır. CI hepsini `--filter "Category!=Integration"` ile hariç tutar.

| Sağlayıcı       | Gerekli env değişkenleri | Script |
|-----------------|---------------------------|--------|
| OpenAI          | `OPENAI_API_KEY`          | `./scripts/run-integration-tests.sh` |
| Mistral         | `MISTRAL_API_KEY`        | `./scripts/run-mistral-integration-tests.sh` |
| Gemini          | `GEMINI_API_KEY`         | `./scripts/run-gemini-integration-tests.sh` |
| Azure OpenAI    | `AZURE_OPENAI_ENDPOINT`, `AZURE_OPENAI_API_KEY` | `./scripts/run-azure-integration-tests.sh` |

Her script, reponun kökünde `.env` varsa yükler ve yalnızca o sağlayıcının entegrasyon testlerini çalıştırır.

### Entegrasyon testlerini çalıştırma (her seferinde)

Reponun kökünden, `.env`’de ilgili anahtarları set ettikten sonra:

```bash
# OpenAI
./scripts/run-integration-tests.sh

# Mistral
./scripts/run-mistral-integration-tests.sh

# Gemini
./scripts/run-gemini-integration-tests.sh

# Azure OpenAI
./scripts/run-azure-integration-tests.sh
```

Veya `dotnet test` ve filter ile (`.env` yüklemeden; değişkenleri shell’de set edin veya script kullanın):

```bash
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName~OpenAIIntegrationTests"
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName~MistralIntegrationTests"
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName~GeminiIntegrationTests"
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName~AzureOpenAIIntegrationTests"
```

**Tüm entegrasyon testlerini hariç tutmak** (örn. anahtar yokken):

```bash
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "Category!=Integration"
```

---

## Kurallar (güvenlik)

- API anahtarınızı **asla** commit etmeyin. Versiyon kontrolü altındaki hiçbir dosyaya yazmayın.
- Anahtar içeren bir dosyayı **asla** push etmeyin. `.env` `.gitignore`’dadır ve commit edilmez.
- Anahtarı **yalnızca** bu repo içinde, kendi makinenizde, test veya VerifyAI çalıştırmak için kullanın.

---

## Özet

| Adım     | Eylem |
|----------|--------|
| Tek seferlik | `cp .env.example .env` → `.env` düzenle, en az bir sağlayıcının anahtarını set et (bkz. `.env.example`) |
| Tümünü doğrula (önerilen) | `dotnet run --project samples/Intentum.VerifyAI` (sağlayıcı başına embedding + tam pipeline) |
| Verbose | `VERIFY_AI_VERBOSE=1 dotnet run --project samples/Intentum.VerifyAI` |
| Sağlayıcı bazlı testler | `./scripts/run-integration-tests.sh` (OpenAI), `run-mistral-integration-tests.sh`, `run-gemini-integration-tests.sh`, `run-azure-integration-tests.sh` |
| Entegrasyon testlerini hariç tut | `dotnet test ... --filter "Category!=Integration"` |
| Asla commit etme | `.env` .gitignore'da; eklemeyin veya push etmeyin. |
