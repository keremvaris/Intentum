# Intentum C# SDK

Auto-generated SDK for the Intentum API.

## Installation

```bash
dotnet add package Intentum.Sdk
```

Or add the generated project to your solution:

```bash
dotnet add reference sdk/csharp/IntentumSdk/IntentumSdk.csproj
```

## Usage

```csharp
using ApiSdk;
using ApiSdk.Models;
using Microsoft.Kiota.Abstractions;

var requestAdapter = new HttpClientRequestAdapter("https://api.intentum.dev");
var client = new IntentumClient(requestAdapter);

var intent = await client.Api.Intent.Infer.PostAsync(new InferRequest
{
    Events = new List<BehaviorEvent>
    {
        new() { Actor = "user", Action = "login", Timestamp = DateTimeOffset.UtcNow }
    }
});

Console.WriteLine($"Intent: {intent.Name} (confidence: {intent.Confidence.Score})");
```

## Requirements

- .NET 10.0 or later
