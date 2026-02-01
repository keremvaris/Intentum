# Test fixtures

## minimal_intent.onnx

Minimal ONNX model for `OnnxIntentModelTests`: input [1,2] float, output [1,3] float (3 classes).

**Generate:** From repo root:

```bash
pip install onnx
python3 scripts/generate_minimal_onnx.py
```

Then run tests; fixture-dependent tests will execute and improve coverage for `OnnxIntentModel`.
