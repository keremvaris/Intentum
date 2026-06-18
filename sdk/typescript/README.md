# Intentum TypeScript SDK

Auto-generated SDK for the Intentum API.

## Installation

```bash
npm install @intentum/sdk
```

Or use the generated package directly.

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

## Requirements

- Node.js 18 or later
- TypeScript 5.0 or later
