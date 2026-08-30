# Intentum Python SDK

Auto-generated SDK for the Intentum API.

## Installation

```bash
pip install intentum-sdk
```

Or use the generated package directly (see Local Development below).

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

## Local Development

To use the generated SDK directly without installing from PyPI:

1. Install required dependencies:

```bash
pip install kiota-abstractions kiota-serialization-json kiota-serialization-text kiota-serialization-form kiota-serialization-multipart kiota-http-httpx
```

2. Add the `sdk/python` directory to your Python path:

```python
import sys
sys.path.insert(0, 'sdk/python')

from intentum_sdk import IntentumClient
```

Or set the `PYTHONPATH` environment variable:

```bash
export PYTHONPATH=/path/to/repo/sdk/python:$PYTHONPATH
```

## Requirements

- Python 3.10 or later
