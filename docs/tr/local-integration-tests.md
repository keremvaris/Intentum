# Yerel entegrasyon testleri (OpenAI)

**OpenAI entegrasyon testleri** (`OpenAIIntegrationTests`), `OPENAI_API_KEY` set edildiğinde gerçek OpenAI API'sini çağırır. Varsayılan olarak CI'da **çalıştırılmaz**; yalnızca kendi makinenizde kullanın.

## Kurallar (güvenlik)

- API anahtarınızı **asla** commit etmeyin. Versiyon kontrolü altındaki hiçbir dosyaya yazmayın.
- Anahtar içeren bir dosyayı **asla** push etmeyin. `.env` `.gitignore`'dadır ve commit edilmez.
- Anahtarı **yalnızca** bu repo içinde, kendi makinenizde, test çalıştırmak için kullanın.

## Tek seferlik kurulum: gizli dosya

1. Şablonu reponun kökünde `.env` olarak kopyalayın:
   ```bash
   cp .env.example .env
   ```
2. `.env` dosyasını düzenleyip anahtarınızı yazın:
   ```
   OPENAI_API_KEY=sk-your-key-here
   ```
   İsteğe bağlı: `OPENAI_BASE_URL` ve `OPENAI_EMBEDDING_MODEL` set edin. Kaydedip kapatın. `.env` gitignore'da.

## Entegrasyon testlerini çalıştırma (her seferinde)

Reponun kökünden:

```bash
./scripts/run-integration-tests.sh
```

Veya bash ile: `bash scripts/run-integration-tests.sh`

Script `.env`'i yükleyip OpenAI entegrasyon testlerini çalıştırır. Her seferinde bir şey `export` etmeniz gerekmez.

## Alternatif: ortam değişkeni terminalde

Dosya kullanmak istemezseniz, değişkeni terminalde set edip testleri çalıştırın:

```bash
export OPENAI_API_KEY='sk-your-key-here'
dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter "FullyQualifiedName~OpenAIIntegrationTests"
```

`.env` içinde isteğe bağlı değişkenler: `OPENAI_BASE_URL` (varsayılan `https://api.openai.com/v1/`), `OPENAI_EMBEDDING_MODEL` (varsayılan `text-embedding-3-small`).

## Özet

| Adım     | Eylem |
|----------|--------|
| Tek seferlik | `cp .env.example .env` → `.env` düzenle, `OPENAI_API_KEY=sk-...` set et |
| Her seferinde | `./scripts/run-integration-tests.sh` (script .env yükleyip testleri çalıştırır) |
| Asla commit etme | `.env` .gitignore'da; eklemeyin veya push etmeyin. |
