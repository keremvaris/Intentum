# IoT Device Risk Intent Detection

Detects IoT device risks: device failure, security breaches, anomalies.

## Run

```bash
dotnet run --project examples/iot-intent
```

No API key required; uses the Mock embedding provider.

## What it does

1. Observes IoT device behavior events (sensors, firmware, network).
2. Infers intent using LlmIntentModel with Mock provider.
3. Applies policy: Block (security breach), Warn (device failure), Observe (anomaly).
