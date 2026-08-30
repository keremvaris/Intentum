# Intentum Python SDK

Auto-generated SDK for the Intentum API.

## Installation

```bash
pip install intentum-sdk
```

Or use the generated package directly (see Local Development below).

## Usage

```python
from kiota_http.httpx_request_adapter import HttpxRequestAdapter
from intentum_sdk.intentum_client import IntentumClient
from intentum_sdk.models import InferRequest, BehaviorEvent

request_adapter = HttpxRequestAdapter(base_url="https://api.intentum.dev")
client = IntentumClient(request_adapter)

result = await client.api.intent.infer.post(InferRequest(
    events=[BehaviorEvent(actor="user", action="login", timestamp="2026-06-18T00:00:00Z")]
))
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
