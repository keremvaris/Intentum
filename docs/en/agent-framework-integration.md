# Agent framework integration (Faz 4)

**Why you're reading this page:** This page outlines how to use Intentum as an intent verification / guardrail layer with agent frameworks (Semantic Kernel, LangChain, AutoGen, CrewAI, LlamaIndex). It is the right place if you want to apply policy decisions to agent actions.

Intentum can act as an **intent verification / guardrail** layer for agent frameworks (Semantic Kernel, LangChain, AutoGen, CrewAI, LlamaIndex). This page outlines how to build an **adapter** or **middleware** that feeds behavior events into Intentum and applies policy decisions to agent actions.

---

## Goal

- **Input:** Agent framework emits events (e.g. "user asked X", "tool called Y", "step completed"). Map these to Intentum **BehaviorSpace** (actor:action).
- **Intentum:** Run `model.Infer(space)` and `intent.Decide(policy)`.
- **Output:** Use the **decision** (Allow, Block, Observe, etc.) to let the agent proceed, block, or escalate; optionally use **intent name** and **confidence** for routing or logging.

---

## Adapter responsibilities

1. **Event mapping:** Translate framework-specific events (e.g. LangChain "tool call", Semantic Kernel "step") into `BehaviorEvent(actor, action, occurredAt)`. Example: actor = "agent", action = "tool.search"; or actor = "user", action = "message.submit".
2. **Space lifecycle:** Create or reuse a `BehaviorSpace` per session or conversation; call `space.Observe(actor, action)` for each event.
3. **Inference:** When you need a decision (e.g. before executing a tool, or after N steps), call `model.Infer(space)` and `intent.Decide(policy)`.
4. **Policy:** Define an `IntentPolicy` that maps intent + confidence to Allow/Block/Observe (e.g. Block when confidence low and signals indicate abuse).

---

## Example interface (pseudo-code)

```csharp
// Conceptual adapter interface (implement per framework)
public interface IIntentumAgentAdapter
{
    void RecordEvent(string actor, string action);
    (Intent Intent, PolicyDecision Decision) InferAndDecide();
}
```

Implementation: maintain a `BehaviorSpace`, implement `RecordEvent` as `space.Observe(actor, action)`, and `InferAndDecide` as `model.Infer(space)` + `intent.Decide(policy)`.

---

## Target frameworks

- **Semantic Kernel (.NET):** Middleware or filter that runs before/after kernel steps; map steps to actor:action, call Intentum, block or allow.
- **LangChain / LlamaIndex (Python):** Would require a Python-side client that calls a small .NET or HTTP service running Intentum, or a Python port of Intentum.

---

## Summary

| Step           | Action |
|----------------|--------|
| Event mapping | Framework events → BehaviorEvent(actor, action) |
| Space         | One BehaviorSpace per session; Observe() each event |
| Inference     | model.Infer(space); intent.Decide(policy) |
| Use decision  | Allow → continue; Block → stop or escalate; Observe → log and continue |

A concrete adapter (e.g. for Semantic Kernel) can be added as a separate repo or sample in the Intentum org. This doc serves as the Faz 4 "agent framework middleware" specification.

**Next step:** When you're done with this page → [Scenarios](scenarios.md) or [API Reference](api.md).
