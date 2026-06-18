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
using Intentum.Sdk;

var client = new IntentumClient("https://api.intentum.dev");

var intent = await client.InferAsync(new InferRequest
{
    Events = new List<BehaviorEvent>
    {
        new("user", "login", DateTimeOffset.UtcNow)
    }
});

Console.WriteLine($"Intent: {intent.Name} (confidence: {intent.Confidence.Score})");
```

## Requirements

- .NET 10.0 or later
