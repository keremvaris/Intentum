# Migration Rehberi

Bu rehber, Intentum sürümleri arasında yükseltme yapmanıza yardımcı olur.

## v1.0 → v1.1

### Breaking Changes

Yok. v1.1, v1.0 ile geriye uyumludur.

### Yeni Özellikler

- **Async Policy Decision**: Bloklamayan policy değerlendirmesi için `DecideAsync` extension metodu
- **CosineSimilarityHelper**: Net [-1,1] ve [0,1] aralıklarıyla birleştirilmiş kosinüs benzerliği
- **Validation Layer**: BehaviorSpace için `EnsureNotEmpty()`, IntentPolicy için `Validate()`
- **Rate Limiting DI**: `AddIntentumRateLimiting()` extension metodu
- **Health Checks**: `IntentModelHealthCheck`, `PolicyEngineHealthCheck`
- **PII Detection**: `BehaviorSpaceSanitization` içinde e-posta ve telefon maskeleme
- **VS Code Snippets**: Intentum geliştirme için kod snippet'leri

### Deprecation (Kullanımdan Kaldırılanlar)

- `Decide()` extension metodu (yerine `DecideAsync` kullanın)
  - `[Obsolete]` attribute ile işaretlendi
  - v2.0'da kaldırılacak

### Migration Adımları

1. NuGet paketlerini güncelleyin:
   ```bash
   dotnet add package Intentum.Core --version 1.1.x
   dotnet add package Intentum.Runtime --version 1.1.x
   ```

2. `Decide()` yerine `DecideAsync()` kullanın:
   ```csharp
   // Önce
   var decision = intent.Decide(policy);

   // Sonra
   var decision = await intent.DecideAsync(policy);
   ```

3. Testleri yeni validasyonla güncelleyin:
   ```csharp
   // Önce
   var intent = model.Infer(space);

   // Sonra (isteğe bağlı, açık validasyon için)
   var intent = model.InferWithValidation(space);
   ```

### Yükseltme Kontrol Listesi

- [ ] NuGet paketlerini güncelle
- [ ] `Decide()` çağrılarını bul ve `DecideAsync()` ile değiştir
- [ ] Testleri çalıştır ve regresyon olmadığını doğrula
- [ ] SonarCloud analizini kontrol et
