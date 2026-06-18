# Migration Guide

This guide helps you upgrade Intentum between versions.

## v1.0 → v1.1

### Breaking Changes

None. v1.1 is backwards compatible with v1.0.

### New Features

- **Async Policy Decision**: `DecideAsync` extension method for non-blocking policy evaluation
- **CosineSimilarityHelper**: Consolidated cosine similarity with clear [-1,1] and [0,1] ranges
- **Validation Layer**: `EnsureNotEmpty()` for BehaviorSpace, `Validate()` for IntentPolicy
- **Rate Limiting DI**: `AddIntentumRateLimiting()` extension method
- **Health Checks**: `IntentModelHealthCheck`, `PolicyEngineHealthCheck`
- **PII Detection**: Email and phone masking in `BehaviorSpaceSanitization`
- **VS Code Snippets**: Code snippets for Intentum development

### Deprecated APIs

- `Decide()` extension method (use `DecideAsync` instead)
  - Marked with `[Obsolete]` attribute
  - Will be removed in v2.0

### Migration Steps

1. Update NuGet packages:
   ```bash
   dotnet add package Intentum.Core --version 1.1.x
   dotnet add package Intentum.Runtime --version 1.1.x
   ```

2. Replace `Decide()` with `DecideAsync()`:
   ```csharp
   // Before
   var decision = intent.Decide(policy);
   
   // After
   var decision = await intent.DecideAsync(policy);
   ```

3. Update tests to use new validation:
   ```csharp
   // Before
   var intent = model.Infer(space);
   
   // After (optional, for explicit validation)
   var intent = model.InferWithValidation(space);
   ```

### Upgrade Checklist

- [ ] Update NuGet packages
- [ ] Search for `Decide()` calls and replace with `DecideAsync()`
- [ ] Run tests to verify no regressions
- [ ] Review SonarCloud analysis for new issues
