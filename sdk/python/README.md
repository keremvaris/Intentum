# Intentum Python SDK

Auto-generated SDK for the Intentum API.

## Installation

```bash
pip install intentum-sdk
```

Or use the generated package directly.

## Usage

```python
from intentum_sdk import IntentumClient

client = IntentumClient("https://api.intentum.dev")

events = [
    {"actor": "user", "action": "login", "timestamp": "2026-06-18T00:00:00Z"}
]

result = client.infer_intent(events)
print(f"Intent: {result.name} (confidence: {result.confidence.score})")
```

## Requirements

- Python 3.10 or later
