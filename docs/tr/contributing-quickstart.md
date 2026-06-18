# Katkıda Bulunma Hızlı Başlangıç

Intentum'a hoş geldiniz! Bu rehber, katkıda bulunmaya başlamanıza yardımcı olur.

## Mimari Özet

```
Intentum.Core          → Core domain (BehaviorSpace, Intent, Confidence)
Intentum.Runtime       → Policy engine, rate limiting
Intentum.AI            → Embeddings, similarity, classification
Intentum.AI.*          → AI provider implementations
Intentum.AspNetCore    → Web integration, health checks
Intentum.Persistence   → Repository interfaces
Intentum.Persistence.* → Persistence implementations
Intentum.Observability → Metrics, tracing
```

## Yeni Domain Kural Seti Ekleme

1. `src/Intentum.Core/{Domain}/` altında yeni bir dosya oluşturun:
   ```csharp
   namespace Intentum.Core.Healthcare;

   public static class HealthcareRules
   {
       public static RuleMatch? PatientDeterioration(BehaviorSpace space)
       {
           var hasVitalAlert = space.Events.Any(e =>
               e.Action == "vital.signs.alert");

           if (hasVitalAlert)
               return new RuleMatch("PatientDeterioration", 0.8);

           return null;
       }
   }
   ```

2. `tests/Intentum.Tests/` içinde test ekleyin:
   ```csharp
   [Fact]
   public void PatientDeterioration_WithVitalAlert_ReturnsMatch()
   {
       var space = new BehaviorSpace();
       space.Observe(new BehaviorEvent("patient", "vital.signs.alert"));

       var match = HealthcareRules.PatientDeterioration(space);

       Assert.NotNull(match);
       Assert.Equal("PatientDeterioration", match.Name);
   }
   ```

3. `docs/tr/domain-intent-templates.md` içinde dokümante edin

## Yeni AI Sağlayıcısı Ekleme

1. `src/Intentum.AI.{Provider}/` projesi oluşturun

2. `IIntentEmbeddingProvider` implemente edin:
   ```csharp
   public class ProviderEmbeddingProvider : IIntentEmbeddingProvider
   {
       public async Task<IntentEmbedding> EmbedAsync(
           string behaviorKey,
           CancellationToken cancellationToken = default)
       {
           var response = await _httpClient.PostAsync(...);
           var embedding = ParseEmbedding(response);
           return new IntentEmbedding(behaviorKey, embedding.Vector, embedding.Score);
       }
   }
   ```

3. DI extension ekleyin:
   ```csharp
   public static IServiceCollection AddProviderEmbedding(
       this IServiceCollection services,
       Action<ProviderOptions> configure)
   {
       services.Configure(configure);
       services.AddSingleton<IIntentEmbeddingProvider, ProviderEmbeddingProvider>();
       return services;
   }
   ```

4. Health check ekleyin

5. Mock HTTP client ile test yazın

## Yeni Persistence Backend'i Ekleme

1. `src/Intentum.Persistence.{Backend}/` projesi oluşturun

2. Repository interface'lerini implemente edin:
   ```csharp
   public class BackendIntentHistoryRepository : IIntentHistoryRepository
   {
       public async Task SaveAsync(Intent intent, CancellationToken cancellationToken = default)
       {
           // Backend'e kaydet
       }

       public async Task<IReadOnlyCollection<Intent>> ListAsync(CancellationToken cancellationToken = default)
       {
           // Backend'den listele
       }
   }
   ```

3. DI extension ekleyin:
   ```csharp
   public static IServiceCollection AddBackendPersistence(
       this IServiceCollection services,
       Action<BackendOptions> configure)
   {
       services.Configure(configure);
       services.AddSingleton<IIntentHistoryRepository, BackendIntentHistoryRepository>();
       return services;
   }
   ```

4. Entegrasyon testleri yazın

## PR Kontrol Listesi

- [ ] Kod mevcut pattern'leri takip ediyor
- [ ] Testler geçiyor (`dotnet test`)
- [ ] Yeni uyarı yok
- [ ] Dokümantasyon güncellendi (varsa)
- [ ] CHANGELOG girdisi eklendi (varsa)
- [ ] Conventional commit mesajı
