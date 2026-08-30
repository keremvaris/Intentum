# Intentum TypeScript SDK

Auto-generated SDK for the Intentum API.

## Installation

```bash
npm install @intentum/sdk
```

Or use the generated package directly (see Local Development below).

## Usage

```typescript
import { IntentumClient } from '@intentum/sdk';

const client = new IntentumClient('https://api.intentum.dev');

const intent = await client.inferIntent({
    events: [
        { actor: 'user', action: 'login' }
    ]
});

console.log(`Intent: ${intent.name} (confidence: ${intent.confidence.score})`);
```

## Local Development

To use the generated SDK directly without publishing to npm:

1. Install required dependencies:

```bash
npm install @microsoft/kiota-abstractions @microsoft/kiota-serialization-json @microsoft/kiota-serialization-text @microsoft/kiota-serialization-form @microsoft/kiota-serialization-multipart
```

2. Import from the generated directory:

```typescript
import { createIntentumClient } from './sdk/typescript/intentum-sdk/intentumClient.js';
```

Or add the SDK directory to your TypeScript project paths in `tsconfig.json`:

```json
{
  "compilerOptions": {
    "baseUrl": ".",
    "paths": {
      "@intentum/sdk": ["sdk/typescript/intentum-sdk"]
    }
  }
}
```

## Requirements

- Node.js 18 or later
- TypeScript 5.0 or later
