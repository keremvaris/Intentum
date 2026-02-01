# Domain intent templates

**Why you're reading this page:** This page provides intent names and actor:action (signal) sets for fraud/security and customer intent. It is the right place if you are asking "How do I define intents for my domain?" in a repeatable way.

Templates for defining **intent names** and **actor:action** (signal) sets for two domains beyond greenwashing: **fraud / security** and **customer intent**. Use these to answer "How do I define intents for my domain?" in a repeatable way.

---

## Fraud / security

**Intent names (suggested):** `SuspiciousAccess`, `CredentialStuffing`, `AccountRecovery`, `NormalLogin`, `HighRiskSession`.

**Suggested actor:action (signals):**

| Actor   | Action (examples)        | Meaning |
|--------|---------------------------|--------|
| user   | login.failed              | Failed login attempt |
| user   | login.retry               | Retry after failure |
| user   | ip.changed                | IP change in session |
| user   | password.reset            | Password reset flow |
| user   | login.success             | Successful login |
| user   | device.verified           | Device verification |
| user   | captcha.passed            | CAPTCHA solved |
| system | velocity.high             | Many requests in short time |

**Policy idea:** Block when confidence high and signal count ≥ N (e.g. many failed logins + IP change); Observe when medium risk; Allow when low. See [examples/fraud-intent](../../examples/fraud-intent/).

---

## Customer intent

**Intent names (suggested):** `PurchaseIntent`, `InfoGathering`, `SupportRequest`, `BrowsingOnly`.

**Suggested actor:action (signals):**

| Actor | Action (examples)   | Meaning |
|-------|--------------------|--------|
| user  | browse.category    | Category browse |
| user  | cart.add           | Add to cart |
| user  | checkout.start     | Start checkout |
| user  | payment.submit     | Submit payment |
| user  | search.product     | Product search |
| user  | view.product       | View product |
| user  | compare.product    | Compare products |
| user  | view.faq           | View FAQ |
| user  | contact.click      | Click contact |
| user  | ticket.create      | Create support ticket |
| user  | chat.start         | Start chat |

**Policy idea:** Allow high confidence; Observe medium; route by intent (purchase → checkout, support → human/chat). See [examples/customer-intent](../../examples/customer-intent/).

---

## How to use

1. Choose a domain (fraud, customer, or your own).
2. Define intent names and map them to confidence thresholds or rules.
3. Record events with `space.Observe(actor, action)` using the suggested actions (or your own).
4. Run your intent model (e.g. `LlmIntentModel` or a rule-based model) and policy; use the intent name + confidence for routing or blocking.

For greenwashing, see [Greenwashing detection how-to](greenwashing-detection-howto.md) and [Greenwashing metrics](../case-studies/greenwashing-metrics.md).

**Next step:** When you're done with this page → [Designing intent models](designing-intent-models.md) or [Scenarios](scenarios.md).
