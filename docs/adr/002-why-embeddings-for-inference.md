# ADR-002: Why Embeddings for Intent Inference

## Status

Accepted

## Context

Intent inference requires understanding the semantic meaning of observed behaviors. Two different behavior sequences might represent the same intent:
- "login.failed → login.retry → login.success" = AccountAccess
- "password.reset → email.confirm → login.success" = AccountAccess

We need a representation that captures semantic similarity, not just string matching.

## Decision

Use embedding vectors to represent behaviors, then compute similarity between behavior spaces and intent definitions.

The pipeline:
1. Convert behavior events to embedding vectors
2. Combine vectors into a behavior space representation
3. Compare with reference intent embeddings
4. Return intent with confidence score

## Alternatives Considered

1. **Keyword matching** - Match behavior strings to intent keywords
   - Rejected: Too brittle, can't handle synonyms or variations

2. **Rule-based systems** - Define rules for each intent
   - Rejected: Doesn't scale, can't handle ambiguity

3. **Direct LLM classification** - Ask LLM to classify intent directly
   - Rejected: Expensive, slow, not deterministic enough for real-time

## Consequences

### Positive
+ Captures semantic similarity
+ Handles behavior variations gracefully
+ Enables few-shot learning
+ Works across languages

### Negative
- Requires embedding model (API dependency)
- Adds latency for embedding computation
- Embedding quality affects accuracy

### Neutral
* Can fall back to rule-based when embeddings unavailable
* Embedding cache reduces cost and latency

## Notes

The Intentum.AI module implements this pipeline with pluggable providers.
