# System One decision engines as Intentum `IIntentModel`

Runs a real inference against any **System One compatible** engine (`POST /v1/systemone`, TypeSafe Jev wire protocol) and maps the answer to an Intentum `Intent` — no Python code in .NET, just HTTP.

Supported engines (one shared client, presets in `SystemOneEngines`):

| Engine | Repo / source | Default endpoint | Start command |
|--------|---------------|------------------|---------------|
| **Jev** (TypeSafe) | [typesafe.ai](https://typesafe.ai) — hosted, closed weights | `https://api.typesafe.ai` | not self-hosted; get a key at [console.typesafe.ai/keys](https://console.typesafe.ai/keys) and export `TYPESAFE_API_KEY` |
| **Kev** | [jaredpalmer/kev](https://github.com/jaredpalmer/kev) | `http://127.0.0.1:8009` | see repo (serve, `KEV_API_KEY` optional) |
| **TinyJev** | [ankit-aglawe/tinyjev](https://github.com/ankit-aglawe/tinyjev) | `http://127.0.0.1:8077` | `tinyjev serve TinyJev-0.6B` |
| **Laya** | [NandhaKishorM/laya](https://github.com/NandhaKishorM/laya) | `http://127.0.0.1:8000` | `pip install "laya[serve]" && laya-serve` |
| **OpenDecision** | System One compatible local server | `http://127.0.0.1:8000` | see its README |
| **Decision 1.0** | [vllm-sr.ai](https://vllm-sr.ai) | `http://127.0.0.1:8000` | see its docs |
| **Open-Jev** | open-jev HTTP server | `http://127.0.0.1:8000` | see its README |
| **NanoJev** | local System One server | `http://127.0.0.1:8000` | see its README |
| **OpenJevPro** | [ekzhang/openjev-sglang](https://github.com/ekzhang/openjev) | deployed URL | `--base-url <url>` |
| **Foq** | System One compatible server | `http://127.0.0.1:8000` | see its README |

Ports other than the verified ones (Kev 8009, TinyJev 8077, Laya 8000) default to 8000 — override with `--base-url`.

## Run

Start an engine first, then from the repo root:

```bash
dotnet run --project examples/system-one-decision -- --engine laya
dotnet run --project examples/system-one-decision -- --engine jev     # hosted Jev (needs TYPESAFE_API_KEY)
dotnet run --project examples/system-one-decision -- --engine kev
dotnet run --project examples/system-one-decision -- --engine tinyjev
dotnet run --project examples/system-one-decision -- --engine openjevpro --base-url https://your-deployment.example/v1
```

Environment variables instead of flags: `SYSTEMONE_ENGINE`, `SYSTEMONE_BASE_URL`.

No API key needed for local engines; set `ApiKey` in `SystemOneOptions` when the engine requires a bearer token (`KEV_API_KEY`, `LAYA_API_KEY`).

## Local engines (kev & laya)

Both engines can run at the same time (ports do not clash). Start each in its own terminal, then run the example against them.

```bash
# Terminal 1 — kev (port 8009): only works from the kev repo
git clone https://github.com/jaredpalmer/kev.git && cd kev
uv sync --extra serve
uv run --extra serve python -m kev.serve --run jaredpalmer/kev-4b --port 8009

# Terminal 2 — laya (port 8000): needs Python >= 3.10
pip install "laya[serve]"
laya-serve
```

> Note: kev downloads a model on first run (GBs); demo engine (`--engine demo` on the web page) needs nothing. If your system Python is older than 3.10, install laya into a uv venv instead: `uv venv ~/venvs/laya --python 3.12 && uv pip install --python ~/venvs/laya/bin/python "laya[serve]"`.

You can also use `./run-engines.sh`, which prepares both engines on first run (clones/syncs kev, builds the laya venv) and opens them in separate Terminal.app windows (macOS; falls back to printed commands elsewhere).

## What it does

1. Builds a sample `BehaviorSpace` (charged twice → viewed invoice → requested refund).
2. Converts the intent catalog (Billing / Technical / Account / Other) into one System One **choice** question.
3. `SystemOneIntentModel.Infer` → HTTP `POST /v1/systemone` with `{state, questions}`.
4. Maps the answer's choice + calibrated probabilities to an Intentum `Intent` (name, per-option signals, confidence level, reasoning).

## Expected output

```
Engine   : laya (http://127.0.0.1:8000)
Behavior :
  user-42:payment.charged_twice
  user-42:invoice.viewed
  user-42:refund.requested

Intent     : Billing
Confidence : 0.93 (Certain)
Reasoning  : choice: Billing=0.93, Technical=0.04, Account=0.02, Other=0.01
  signal   : Billing = 0.93 [laya]
  ...
```

## Laya notes

Laya is non-autoregressive (single forward pass, ~33 ms on GPU): typed `choice` / `score` / `noul` answers, 100+ languages, and a router that picks a checkpoint per request (`english`, `multilingual`, `typed-decisions`).

- `laya-serve` speaks the same `/v1/systemone` protocol as Jev, so this example works unchanged against it.
- Set `Model` in `SystemOneOptions` to pin a checkpoint (`english`, `multilingual`, `typed-decisions`); otherwise Laya's router picks one.
- If you set `LAYA_API_KEY`, also set `ApiKey = "..."` in the options.

## Using it in your own code

```csharp
using Intentum.AI.SystemOne;

var options = SystemOneEngines.Laya() with
{
    IntentCatalog = new Dictionary<string, string>
    {
        ["Billing"] = "invoices, charges, refunds",
        ["Technical"] = "bugs, outages, errors",
    },
};

IIntentModel model = new SystemOneIntentModel(options, new HttpClient());
var intent = model.Infer(behaviorSpace);
```

DI: `services.AddIntentumSystemOne(options);`
