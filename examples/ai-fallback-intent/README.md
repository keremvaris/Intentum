# AI Decision Fallback & Validation

This example shows Intentum for **AI decision fallback**: inferring whether the model's behavior indicates rushed classification (e.g. high confidence, short reasoning, user rephrased, model changed answer) vs careful understanding (clarifying questions, explicit reasoning, moderate confidence), then feeding a policy decision (Block → route to human, Allow → auto decision).

## Run

```bash
dotnet run --project examples/ai-fallback-intent
```

No API key required; uses the Mock embedding provider.

## What it does

1. **Observed AI behavior** — LLM signals: high_confidence, short_reasoning, no_followup_question, user rephrased_request, llm changed_answer; or asked_clarifying_question, provided_details, reasoning_explicit, moderate_confidence.
2. **Infer intent** — `LlmIntentModel` (Mock) produces confidence and signals from the behavior space.
3. **Decide** — Policy maps to Block (route to human) or Allow (auto decision). In production you would call RouteToHuman() or AllowAutoDecision().

## Docs

See [Real-world scenarios — AI Decision Fallback](https://github.com/keremvaris/Intentum/blob/master/docs/en/real-world-scenarios.md#use-case-2-ai-decision-fallback--validation) in the documentation.
