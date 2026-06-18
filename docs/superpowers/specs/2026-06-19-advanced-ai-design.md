# Advanced AI Features Design Spec

## Goal

Add 5 advanced AI capabilities (Confidence Calibration, Few-Shot Learning, Multi-Modal Fusion, Ensemble Models, Token Cost Tracking) to the Intentum AI layer.

## Context

Intentum.AI currently has embeddings, similarity engines, LLM integration, and an intent catalog. Missing: confidence calibration (only static thresholds), few-shot learning (catalog is static), multi-modal support, model-level ensemble, and token cost tracking. Each feature goes in its own folder under `src/Intentum.AI/`, following existing patterns (interface + implementation).

## Architecture

```
src/Intentum.AI/
├── Calibration/
│   ├── IConfidenceCalibrator.cs
│   ├── PlattCalibrator.cs
│   └── TemperatureCalibrator.cs
├── FewShot/
│   ├── IFewShotStore.cs
│   ├── MemoryFewShotStore.cs
│   ├── FewShotIntentModel.cs
│   └── FewShotExtensions.cs
├── MultiModal/
│   ├── IMultiModalInput.cs
│   ├── MultiModalFusion.cs
│   └── MultiModalIntentModel.cs
├── Ensemble/
│   ├── IEnsembleStrategy.cs
│   ├── WeightedEnsemble.cs
│   ├── MajorityVotingEnsemble.cs
│   └── EnsembleIntentModel.cs
├── TokenCost/
│   ├── ITokenCounter.cs
│   ├── ITokenCostTracker.cs
│   └── MemoryTokenCostTracker.cs
```

## Features

### 1. Confidence Calibration
- `IConfidenceCalibrator` — `double Calibrate(double rawScore)`
- `PlattCalibrator` — sigmoid: `1 / (1 + exp(a * rawScore + b))`
- `TemperatureCalibrator` — `softmax(logits / T)`, higher T = more uniform
- Default no-op calibrator returns raw score unchanged

### 2. Few-Shot Learning
- `FewShotExample(string IntentName, string[] BehaviorKeys, double Confidence)`
- `IFewShotStore` — `AddExample()`, `FindSimilar(keys, topK)`, `Clear()`
- `MemoryFewShotStore` — in-memory implementation
- `FewShotIntentModel` — IIntentModel: finds similar examples, returns best match
- DI extension for registration

### 3. Multi-Modal Fusion
- `InputModality` enum: Behavior, Image, Audio, Text
- `MultiModalInput(InputModality, string Value, float[]? Embedding)`
- `MultiModalFusion` — fuses behavior embedding + additional modality embeddings
- `MultiModalIntentModel` — IIntentModel: accepts additional inputs, fuses and infers

### 4. Ensemble Models
- `ModelResult(string Name, double Score, double Weight)`
- `IEnsembleStrategy` — `Intent Combine(ModelResult[])`
- `WeightedEnsemble` — weighted average of scores
- `MajorityVotingEnsemble` — majority vote on intent names
- `EnsembleIntentModel` — IIntentModel: runs N IIntentModel instances, combines via strategy

### 5. Token Cost Tracking
- `TokenCost(string Model, int PromptTokens, int CompletionTokens, decimal Cost)`
- `ITokenCounter` — `int Count(string text)`
- `ITokenCostTracker` — `Track(TokenCost)`, `GetTotal()`, `Reset()`
- `MemoryTokenCostTracker` — in-memory implementation

## Testing Strategy

Each feature gets unit tests:
- Calibration: Platt sigmoid calculation, temperature scaling, edge cases (0, 1)
- FewShot: store/retrieve, similarity search, empty store
- MultiModal: fusion dimensions, missing modalities
- Ensemble: weighted average, majority vote, tied votes
- TokenCost: counting, accumulation, reset

## Acceptance Criteria

1. All 5 features have interface + implementation
2. All features build with 0 errors, 0 warnings
3. All existing tests still pass
4. No new NuGet dependencies required
5. All features follow existing Intentum.AI patterns

## Files

### New Source Files (~18 files)
- `src/Intentum.AI/Calibration/IConfidenceCalibrator.cs`
- `src/Intentum.AI/Calibration/PlattCalibrator.cs`
- `src/Intentum.AI/Calibration/TemperatureCalibrator.cs`
- `src/Intentum.AI/FewShot/IFewShotStore.cs`
- `src/Intentum.AI/FewShot/MemoryFewShotStore.cs`
- `src/Intentum.AI/FewShot/FewShotIntentModel.cs`
- `src/Intentum.AI/FewShot/FewShotExtensions.cs`
- `src/Intentum.AI/MultiModal/IMultiModalInput.cs`
- `src/Intentum.AI/MultiModal/MultiModalFusion.cs`
- `src/Intentum.AI/MultiModal/MultiModalIntentModel.cs`
- `src/Intentum.AI/Ensemble/IEnsembleStrategy.cs`
- `src/Intentum.AI/Ensemble/WeightedEnsemble.cs`
- `src/Intentum.AI/Ensemble/MajorityVotingEnsemble.cs`
- `src/Intentum.AI/Ensemble/EnsembleIntentModel.cs`
- `src/Intentum.AI/TokenCost/ITokenCounter.cs`
- `src/Intentum.AI/TokenCost/ITokenCostTracker.cs`
- `src/Intentum.AI/TokenCost/MemoryTokenCostTracker.cs`

### New Test Files (~5 files)
- `tests/Intentum.Tests/AI/CalibrationTests.cs`
- `tests/Intentum.Tests/AI/FewShotTests.cs`
- `tests/Intentum.Tests/AI/MultiModalTests.cs`
- `tests/Intentum.Tests/AI/EnsembleTests.cs`
- `tests/Intentum.Tests/AI/TokenCostTests.cs`
