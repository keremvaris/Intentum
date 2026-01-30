# Vector Normalization

This example shows **ToVector(options)** for normalizing behavior vectors: Cap per dimension, L1 norm, or SoftCap. Prevents a single repeated event (e.g. many `login.failed`) from dominating the vector.

## Run

```bash
dotnet run --project examples/vector-normalization
```

No dependencies beyond Intentum.Core.

## What it does

1. **Raw** — `ToVector()` or `ToVector(null)`: actor:action → count (no normalization).
2. **Cap** — `ToVector(new ToVectorOptions(VectorNormalization.Cap, CapPerDimension: 3))`: each dimension capped at 3.
3. **L1** — `ToVector(new ToVectorOptions(VectorNormalization.L1))`: scale so sum of dimension values = 1.
4. **SoftCap** — `ToVector(new ToVectorOptions(VectorNormalization.SoftCap, CapPerDimension: 3))`: value/cap, min 1.

## Docs

- [Advanced features — Behavior vector normalization](https://github.com/keremvaris/Intentum/blob/master/docs/en/advanced-features.md#behavior-vector-normalization)
- [API — ToVectorOptions, BehaviorSpace.ToVector](https://github.com/keremvaris/Intentum/blob/master/docs/en/api.md)
