# What These Features Do — Simple Guide

For someone new to Intentum: what each feature is for, in plain language.

---

## Intent Timeline

**What it is:** A time-ordered history of “what did this user (or session) intend?” over a period.

**Why it’s useful:**  
You record intent results with an optional **entity id** (e.g. user id, session id). Later you can ask: “How did this entity’s intent evolve?” — e.g. Monday: Low confidence → Observe; Tuesday: High → Allow.  
Useful for **support** (“what did this user do before the block?”), **auditing**, and **dashboards** (e.g. “intent over time” chart per user).

**In one sentence:** *“Show me this user’s intent history over time.”*

---

## Intent Tree

**What it is:** An explanation of **why** the system decided Allow, Block, Observe, etc. — in the form of a **decision tree**: which rule matched, which signals (behaviors) contributed.

**Why it’s useful:**  
Users and auditors want to know *why* a decision was made. The tree shows the path: “Rule X matched because confidence was High and signal Y had weight Z.”  
Useful for **transparency**, **compliance**, **support** (“why was I blocked?”), and **debugging** (tuning rules).

**In one sentence:** *“Show me why this decision was made (which rule, which signals).”*

---

## Context-Aware Policy

**What it is:** Policy rules that depend not only on the **current intent**, but on **context**: e.g. server load, region, recent intents (e.g. “same intent 3 times in a row”), or custom data.

**Why it’s useful:**  
Sometimes the right decision depends on the situation: e.g. “Block if load &gt; 80%” or “Escalate if the user got the same intent three times (maybe stuck).”  
Context = **load**, **region**, **recent intents**, **custom key-value**.  
Useful for **adaptive behavior** and **operational rules** (e.g. overload, A/B by region).

**In one sentence:** *“Decide using not only intent, but also load, region, and recent history.”*

---

## Policy Store

**What it is:** Policies defined in **JSON (or a file)** instead of code. You can **edit rules** (conditions, decisions) in that file; the app can **reload** them without redeploying.

**Why it’s useful:**  
Non-developers (e.g. ops, compliance) can change rules: “Block when confidence is Low” → “Block when confidence is Low **or** signal count &gt; 10.” No code change, no new deploy.  
Useful for **faster rule updates** and **low-code** policy management.

**In one sentence:** *“Change policy rules in a file; no code deploy needed.”*

---

## Behavior Pattern Detector

**What it is:** Analyzes **intent history** to find **patterns** (e.g. “many Blocks on Mondays”) and **anomalies** (e.g. “sudden spike in Block rate” or “unusual confidence distribution”).

**Why it’s useful:**  
You get a lot of intent data over time. This feature helps you **see patterns** and **flag anomalies** — e.g. for monitoring, tuning policies, or detecting abuse.  
Useful for **dashboards**, **alerts**, and **improving** your model/policy.

**In one sentence:** *“Find patterns and anomalies in intent history.”*

---

## Multi-Stage Model

**What it is:** Several intent models in a **chain**, with **confidence thresholds**. Try the first (e.g. fast/cheap rules); if confidence is below the threshold, try the next (e.g. lightweight LLM); if still low, try the last (e.g. heavy LLM).

**Why it’s useful:**  
Heavy models are slow and costly. Multi-stage uses them **only when needed** (low confidence). Most traffic can be handled by rules or a cheap model.  
Useful for **cost** and **latency** without losing quality on hard cases.

**In one sentence:** *“Use cheap/fast model first; use expensive model only when confidence is low.”*

---

## Scenario Runner

**What it is:** Run **predefined scenarios** (e.g. “user does login, login, login, then submit”) through your intent model and policy. You get a result per scenario (e.g. Allow/Block).

**Why it’s useful:**  
You want to **test** that the system behaves as expected: “If someone does X, we should Block.” Scenario Runner runs many such scenarios in one go — for **regression tests**, **demos**, and **validation**.

**In one sentence:** *“Run predefined behavior scenarios and see Allow/Block (for testing and demos).”*

---

## Stream (Real-time intent stream)

**What it is:** Process **behavior events as they arrive** (like a conveyor belt) instead of loading all events into memory at once. You get **batches** of events; for each batch you infer intent and apply policy.

**Why it’s useful:**  
In real-time pipelines (e.g. message queue, event hub), events never “end” — they keep coming. Stream lets you **consume and process** them in batches without holding everything in memory.  
Useful for **workers**, **event-driven apps**, and **high-volume** pipelines.

**In one sentence:** *“Process behavior events as a continuous stream (batches), not all at once.”*

---

## Playground

**What it is:** **Compare** how **different intent models** behave on the **same** input (same list of events). You send one request with events and model names (e.g. “Default”, “Mock”); you get back intent and decision **per model**.

**Why it’s useful:**  
You might have several models (e.g. rule-based, Mock, real LLM). Playground lets you see “for this exact input, what does each model return?” — for **debugging**, **comparison**, and **choosing** the right model.

**In one sentence:** *“Compare different models on the same events (intent + decision per model).”*

---

## Playground: UI

**Current state:** The Sample Web has a **Playground** section in the UI (Examples → Playground): enter events (or pick a preset), check models (Default, Mock), click **Compare**, see a table per model. API: `POST /api/intent/playground/compare`. There is **a Playground section in the UI** for it yet (no “Playground” tab or form in the sample app).

**Done.** The Playground section is in the Sample Web UI: e.g. a form where you enter events (or pick a preset), choose model names (e.g. Default, Mock), click “Compare”, and see a table or cards with intent and decision per model. That would give a visual way to try the feature without calling the API by hand.

---

## Summary table

| Feature | In one sentence |
|--------|-------------------|
| **Intent Timeline** | Show this user’s intent history over time. |
| **Intent Tree** | Show why this decision was made (rule + signals). |
| **Context-Aware Policy** | Decide using intent + load, region, recent history. |
| **Policy Store** | Change policy rules in a file; no code deploy. |
| **Behavior Pattern Detector** | Find patterns and anomalies in intent history. |
| **Multi-Stage Model** | Use cheap model first; expensive model only when needed. |
| **Scenario Runner** | Run predefined scenarios and see Allow/Block (testing/demos). |
| **Stream** | Process events as a continuous stream (batches). |
| **Playground** | Compare different models on the same events (API today; UI can be added). |
