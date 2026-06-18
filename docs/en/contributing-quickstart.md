# Contributing Quick Start

Welcome to Intentum! This guide helps you get started as a contributor.

## Architecture Overview

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

## How to Add a New Domain Rule Set

1. Create a new file in `src/Intentum.Core/{Domain}/`:
   ```csharp
   namespace Intentum.Core.Healthcare;

   public static class HealthcareRules
   {
       public static RuleMatch? PatientDeterioration(BehaviorSpace space)
       {
           // Check for vital signs alerts
           var hasVitalAlert = space.Events.Any(e =>
               e.Action == "vital.signs.alert");

           if (hasVitalAlert)
               return new RuleMatch("PatientDeterioration", 0.8);

           return null;
       }
   }
   ```

2. Add tests in `tests/Intentum.Tests/`:
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

3. Document in `docs/en/domain-intent-templates.md`

## How to Add a New AI Provider

1. Create a new project `src/Intentum.AI.{Provider}/`

2. Implement `IIntentEmbeddingProvider`:
   ```csharp
   public class ProviderEmbeddingProvider : IIntentEmbeddingProvider
   {
       public async Task<IntentEmbedding> EmbedAsync(
           string behaviorKey,
           CancellationToken cancellationToken = default)
       {
           // Call provider API
           var response = await _httpClient.PostAsync(...);

           // Parse response
           var embedding = ParseEmbedding(response);

           return new IntentEmbedding(behaviorKey, embedding.Vector, embedding.Score);
       }
   }
   ```

3. Add DI extension:
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

4. Add health check

5. Write tests with mock HTTP client

## How to Add a New Persistence Backend

1. Create a new project `src/Intentum.Persistence.{Backend}/`

2. Implement repository interfaces:
   ```csharp
   public class BackendIntentHistoryRepository : IIntentHistoryRepository
   {
       public async Task SaveAsync(Intent intent, CancellationToken cancellationToken = default)
       {
           // Save to backend
       }

       public async Task<IReadOnlyCollection<Intent>> ListAsync(CancellationToken cancellationToken = default)
       {
           // List from backend
       }
   }
   ```

3. Add DI extension:
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

4. Write integration tests

## PR Checklist

- [ ] Code follows existing patterns
- [ ] Tests pass (`dotnet test`)
- [ ] No new warnings
- [ ] Documentation updated (if applicable)
- [ ] CHANGELOG entry added (if applicable)
- [ ] Conventional commit message
