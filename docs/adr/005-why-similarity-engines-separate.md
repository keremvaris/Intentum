# ADR-005: Why Similarity Engines Separate

## Status

Accepted

## Context

Intent inference requires combining multiple embeddings into a score. Different strategies exist:
- Simple average
- Weighted average
- Time decay
- Cosine similarity

We need a pluggable architecture for these strategies.

## Decision

Separate similarity engines into `IIntentSimilarityEngine` implementations:
- `SimpleAverageSimilarityEngine`
- `WeightedAverageSimilarityEngine`
- `TimeDecaySimilarityEngine`
- `CosineSimilarityEngine`
- `CompositeSimilarityEngine`

Each engine implements a single strategy and can be composed.

## Alternatives Considered

1. **Single engine with configuration** - One engine with strategy parameter
   - Rejected: Violates Single Responsibility, hard to extend

2. **Extension methods** - Static methods for each strategy
   - Rejected: Can't inject dependencies, hard to test

3. **Strategy pattern in IntentModel** - Embed in model classes
   - Rejected: Couples inference with similarity calculation

## Consequences

### Positive
+ Single Responsibility Principle
+ Easy to add new strategies
+ Easy to test in isolation
+ Supports composition via CompositeSimilarityEngine

### Negative
- More classes to maintain
- Requires DI registration

### Neutral
* Can be used independently of intent models
* Supports caching per engine

## Notes

The `Intentum.AI.Similarity` namespace contains all implementations.
