# Load test: Sample.Blazor infer endpoint

Short load test for `POST /api/intent/infer` (Sample.Blazor) to document p95 latency and error rate.

## How the script works

The script [scripts/load-test-infer.js](../../scripts/load-test-infer.js) is a **k6** load test (not Node.js or Bun). It:

1. **Engine:** Uses [k6](https://k6.io/) (Grafana k6). Install with: `brew install k6` (macOS) or see [k6 installation](https://k6.io/docs/get-started/installation/).
2. **Target:** Sends `POST /api/intent/infer` with a fixed JSON body: `{ "events": [ { "actor": "user", "action": "login" }, { "actor": "user", "action": "submit" } ] }`.
3. **Load:** 50 virtual users (VUs), 30 seconds, ~0.1 s sleep between iterations.
4. **Thresholds:** Fails the run if error rate ≥ 5% or p95 latency ≥ 2 s.
5. **Base URL:** From env `K6_BASE_URL` or default `http://localhost:5000`.

So you must have **Sample.Blazor running** and **k6 installed** before running the script.

## How to run

1. Install k6 (if needed): `brew install k6`.
2. Start Sample.Blazor: `dotnet run --project samples/Intentum.Sample.Blazor/Intentum.Sample.Blazor.csproj` (default: http://localhost:5018).
3. From repo root run k6: `k6 run scripts/load-test-infer.js`.
4. Optional: override base URL: `K6_BASE_URL=http://localhost:5001 k6 run scripts/load-test-infer.js`.

## Result summary (example)

| Metric           | Target   | Example (run locally) |
|------------------|----------|------------------------|
| **p95 latency**  | &lt; 2s  | ~XX ms                 |
| **Error rate**   | &lt; 5%  | ~0%                    |
| **RPS**          | —        | ~XX                    |

*(Fill in after running `k6 run scripts/load-test-infer.js`; k6 prints summary to stdout.)*

Use these numbers for [production readiness](../en/production-readiness.md) and capacity planning.
