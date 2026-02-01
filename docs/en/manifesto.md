# The Intentum Manifesto

**Why you're reading this page:** This page summarizes Intentum's philosophical foundation: why intent-driven development, why behavior ≠ intent, and why this approach matters in the AI era. It is the right place if you want a deeper answer to "Why Intentum?"

**Intent-Driven Development for the AI Era**

---

## 1. Software is no longer deterministic.

Modern software systems are no longer a sequence of predictable steps.
They are probabilistic, adaptive, and influenced by context, history, and uncertainty.

Yet we still test them as if they were linear scripts.

This is the core mismatch.

---

## 2. Behavior is not intent.

Traditional development asks:

*What did the user do?*

Intentum asks:

*What was the user trying to achieve?*

Actions are symptoms.
Intent is the cause.

Any system that reasons only about behavior is blind to meaning.

---

## 3. Scenarios are brittle. Intent is resilient.

BDD scenarios break when:

- flows change
- features evolve
- AI decisions shift
- edge cases multiply

Intent does not.

Intent survives:

- retries
- errors
- partial success
- alternative paths

Scenarios describe paths.
Intent describes direction.

---

## 4. Tests should describe spaces, not scripts.

A test is not a story.

A test is a behavior space:

- signals
- probabilities
- confidence
- tolerance

Intentum treats correctness as a distribution, not a boolean.

Because in intelligent systems, correctness is never absolute.

---

## 5. AI systems cannot be validated with "Given–When–Then".

Given–When–Then assumes:

- clear inputs
- deterministic transitions
- binary outcomes

AI breaks all three.

Intentum replaces:

- **Given** → Observed signals
- **When** → Behavior evolution
- **Then** → Intent confidence

This is not a refactor.
This is a paradigm shift.

---

## 6. Intent is the new contract.

APIs used to expose functions.
Then they exposed events.
Now they must expose intent.

Intent becomes:

- the boundary
- the expectation
- the invariant

If behavior changes but intent remains aligned, the system is correct.

---

## 7. Failures are signals, not violations.

In Intent-Driven Development:

- failures are data
- retries are information
- anomalies are context

Systems should learn from deviation, not collapse because of it.

---

## 8. We design for understanding, not control.

Control is an illusion in adaptive systems.

Understanding is not.

Intentum exists to help systems:

- infer meaning
- reason under uncertainty
- act with confidence, not certainty

Intentum is not an alternative to BDD.
It is what BDD evolves into when AI enters the system.

---

## One-line philosophy

> **Software should be judged by intent, not by events.**

For a concise rule list, see [The Intentum Canon](intentum-canon.md) (ten principles).

---

## When Intentum is not for you

Intentum is intentionally opinionated.

If your system is:

- deterministic
- static
- rule-based

You may not need it.

But if your system involves:

- AI
- adaptive logic
- probabilistic decisions
- human ambiguity

Then Intent-Driven Development is not optional.
It is inevitable.

**Next step:** When you're done with this page → [Why Intentum](why-intentum.md) or [Intentum Canon](intentum-canon.md).
