# Hello Intentum

Minimal "5-minute" example: **one signal**, **one intent**, **console output**.

## Run

```bash
dotnet run --project examples/hello-intentum
```

## What it does

1. Observes one behavior event: `user` says `hello`.
2. Uses a rule-based intent model: if action is `hello` → intent `Greeting` with 0.9 confidence.
3. Applies a simple policy: allow `Greeting`, observe everything else.
4. Prints intent name, confidence, decision, and reasoning to the console.

## Next steps

- [vector-normalization](https://github.com/keremvaris/Intentum/tree/master/examples/vector-normalization) — behavior vector normalization
- [time-decay-intent](https://github.com/keremvaris/Intentum/tree/master/examples/time-decay-intent) — time decay for session-based intent
- [Examples overview](https://github.com/keremvaris/Intentum/blob/master/docs/en/examples-overview.md)
