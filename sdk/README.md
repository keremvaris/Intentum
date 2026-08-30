# Intentum SDKs

Auto-generated client SDKs for the Intentum API.

## Available SDKs

| Language | Directory | Status |
|----------|-----------|--------|
| C# | `csharp/` | Generated |
| Python | `python/` | Generated |
| TypeScript | `typescript/` | Generated |

## Prerequisites

- [Microsoft Kiota](https://learn.microsoft.com/en-us/openapi/kiota/) CLI tool
- .NET 10.0 SDK (for C# SDK)

## Generating SDKs

**PowerShell (Windows/Linux):**

```powershell
cd sdk
./generate.ps1
```

**Bash (macOS/Linux):**

```bash
cd sdk
./generate.sh
```

Both scripts perform the same operations:
1. Validate the OpenAPI spec exists
2. Check for Kiota CLI
3. Generate C#, Python, and TypeScript SDKs
4. Verify generated output directories

### Options

| Parameter | Default | Description |
|-----------|---------|-------------|
| `$1` / `$SpecPath` | `../docs/openapi/intentum.yaml` | Path to OpenAPI spec |

## API Reference

See the [OpenAPI Specification](../docs/openapi/intentum.yaml) for the full API reference.
