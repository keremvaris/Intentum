# Audience, project types & example use cases (EN)

This page describes **which types of projects** use Intentum, **who the users** are (developer profiles), and **example test cases** at low, medium, and high complexity — for both **AI-driven** and **non-AI (rule-based)** usage. It also gives **sector-based** examples (ESG, Carbon Accounting, Compliance) so you can map Intentum to your domain.

For the core flow (Observe → Infer → Decide), see [index](index.md) and [API Reference](api.md). For runnable scenarios, see the [sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) and [Scenarios](scenarios.md).

---

## Project types: where Intentum fits

| Project type | Typical use | Why Intentum |
|--------------|-------------|--------------|
| **ESG & Sustainability** | ESG reporting, compliance, metric tracking, risk assessment | Observe ESG events (report preparation, compliance review, verification); infer intent; policy decides Allow / Block / Observe. Non-deterministic (multiple stakeholders, compliance checks). |
| **Carbon Accounting** | Carbon footprint calculation, verification, audit trails | Observe carbon calculation and verification events; infer intent; policy decides allow / flag / block based on compliance. |
| **Compliance & audit** | ICMA, LMA compliance checks, audit trails, risk flags | Observe compliance events; infer risk level; policy decides allow / flag / block. |
| **E‑commerce** | Add to cart, checkout, payment validation, retries | Observe cart/checkout/payment events; infer intent; policy allow / observe / block (Block on excessive retries). |
| **Financial reporting** | ESG report submission, validation retries, multi-actor approvals | Observe reporting events and retries; infer intent; policy decides allow / observe / block. |
| **Regulatory workflows** | Multi-stakeholder approvals, compliance verification, risk assessment | Observe workflow events (analyst, compliance, regulator, board); infer intent; policy decides allow / observe / warn / block. |

---

## User profiles: who uses Intentum

| Profile | Role | Typical need |
|---------|------|--------------|
| **Backend / full-stack developer** | Implements Observe (event capture), policy rules, and integration with AI providers | Clear API (BehaviorSpace, Infer, Decide), provider options, env config. |
| **Product / platform** | Defines “what behavior we care about” and “when to allow / block / observe” | Scenarios and policy examples; low/medium/high examples; sector mapping. |
| **Security / risk** | Defines Block rules, thresholds, and audit | Policy order (Block first), retry/rate limits, localization for audit messages. |
| **QA / test** | Validates behavior → intent → decision for key flows | Test cases (low/medium/high), mock provider, contract tests, sector scenarios. |
| **DevOps / SRE** | Runs services with API keys, regions, rate limits | Env vars, provider choice, no raw logging in prod. |

---

## Example test cases by level

These are **example use cases** you can implement and test with Intentum. They are grouped by **complexity** (low, medium, high) and by **usage** (AI-driven vs rule-based / normal). The [sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) and test project implement many of these as runnable scenarios and unit tests.

### Low complexity (3–4 examples)

| # | Name | Behavior (Observe) | Policy idea | Expected | AI / normal |
|---|------|--------------------|------------|----------|-------------|
| 1 | **Carbon footprint calculation** | `analyst:calculate_carbon` → `system:report_generated` | Allow when confidence High | Allow | Both |
| 2 | **ESG metric view** | `user:view_esg_metric` | Low confidence → Warn | Warn | Both |
| 3 | **Add to cart / product view** | `user:view_product` → `user:add_to_cart` | Allow when confidence High | Allow or Observe | Both |
| 4 | **Compliance check** | `compliance:check_icma` → `system:validated` | First rule: Low → Warn | Warn | Normal (rule-based) |

### Medium complexity (3–4 examples)

| # | Name | Behavior (Observe) | Policy idea | Expected | AI / normal |
|---|------|--------------------|------------|----------|-------------|
| 1 | **ESG report submission with retries** | prepare_esg_report → retry_validation → retry_validation → report_submitted | Allow High; Observe Medium; Block only if retry ≥ 3 | Allow or Observe | Both |
| 2 | **Carbon verification process** | verify_carbon_data → request_correction → submit_correction → approve | Allow on approval; Observe on corrections | Allow or Observe | Both |
| 3 | **LMA loan compliance check** | check_lma_compliance → flag_issue → resolve → compliance_ok | Block or Warn on compliance issues | Block or Warn | Normal (rule-based) |
| 4 | **E‑commerce checkout with retries** | cart → checkout → retry → submit | Allow High; Block if excessive retries | Allow or Observe | Both |

### High complexity (3–4 examples)

| # | Name | Behavior (Observe) | Policy idea | Expected | AI / normal |
|---|------|--------------------|------------|----------|-------------|
| 1 | **ESG compliance audit trail** | prepare_esg_report, compliance:review_esg, flag_discrepancy, retry_correction, approve, publish_esg | Block when compliance risk + excessive retries; else Allow High | Block or Allow | Both |
| 2 | **Carbon accounting with multiple validators** | calculate_carbon, internal_audit:review, external_verifier:verify, request_changes, update, certify | Infer intent from embeddings; policy by confidence and signal counts | Allow / Observe / Warn | AI |
| 3 | **ESG risk assessment with multiple stakeholders** | assess_esg_risk, risk_committee:review, request_details, provide_details, approve, board:final_approval | Block on compliance risk; Observe on Medium; Allow on High | Block or Observe or Allow | Both |
| 4 | **E‑commerce checkout with payment validation** | cart, checkout, payment_attempt, retry, payment_validate, submit | Block on excessive retries; Allow on High | Block or Allow or Observe | Both |

---

## Sector-based example usages

| Sector | Example flow | Observe | Infer | Decide |
|--------|--------------|--------|-------|--------|
| **ESG** | ESG report happy path | prepare_esg_report, compliance:approve, publish_esg | Intent + confidence | Allow / Observe |
| **ESG** | ESG report with compliance issues | prepare_esg_report, flag_issue, retry_correction×2, approve | Intent + signals | Block if excessive retries; else Allow |
| **Carbon** | Carbon calculation success | calculate_carbon, validate, record | Intent + confidence | Allow |
| **Carbon** | Carbon verification with corrections | calculate_carbon, verify, request_correction, correct, approve | Intent | Allow or Observe |
| **Compliance** | Compliance check | check_icma, validated | Risk intent | Warn or Allow |
| **EU Green Bond** | Draft → InProgress → UnderReview → Approved → Completed | process:Draft, InProgress, UnderReview, Approved, Completed | Intent + signals | Allow on Completed |
| **EU Green Bond** | Rejected path | process:Draft, InProgress, UnderReview, Rejected | Intent + signals | Block on Rejected |
| **Classic (Fintech)** | Payment happy path / with retries | login, retry, submit | Intent + confidence | Allow / Observe; Block if excessive retries |
| **Classic (Support)** | Escalation | user:ask, user:ask, system:escalate | Intent | Warn or Allow |
| **Classic (E‑commerce)** | Add to cart / product view | view_product, add_to_cart | Intent + confidence | Allow or Observe |
| **Classic (E‑commerce)** | Checkout success | cart, checkout, submit | Intent | Allow or Observe |
| **Classic (E‑commerce)** | Checkout with retries / payment validation | cart, checkout, retry, payment_validate, submit | Intent + signals | Allow / Observe; Block if excessive retries |

---

## Workflow process status (Draft → InProgress → UnderReview → Approved / Rejected → Completed)

Many complex workflows (LMA, EU Green Bond, ESG reporting, compliance) use a **process status** lifecycle. You Observe status transitions; the model infers intent; policy decides Allow / Block / Observe.

| Status / transition | Observe | Policy idea | Expected |
|---------------------|--------|-------------|----------|
| **Draft → InProgress** | `process:Draft`, `process:InProgress` | Allow or Observe (work in progress) | Allow or Observe |
| **Draft → InProgress → UnderReview → Approved** | Draft, InProgress, UnderReview, Approved | Allow when Approved signal present | Allow |
| **Draft → InProgress → UnderReview → Approved → Completed** | Full lifecycle | Allow when Completed | Allow |
| **Draft → InProgress → UnderReview → Rejected** | Draft, InProgress, UnderReview, Rejected | Block when Rejected | Block |
| **Stuck in Draft / InProgress** | Draft, InProgress only | Observe (no Approved/Rejected/Completed) | Observe or Allow |

The sample and **WorkflowStatusTests** include these transitions for ESG report, compliance, LMA loan, and EU Green Bond style workflows.

---

## How the sample and tests map to this

- **Sample project** runs: ESG/Carbon/EU Green Bond scenarios; **workflow process status** (Draft, InProgress, UnderReview, Approved, Rejected, Completed) for EU Green Bond, ESG, compliance, LMA; and **classic** examples (payment happy path, payment with retries, suspicious retries, support escalation, e‑commerce: add to cart, checkout success, checkout with retries, payment validation).
- **Test project** has: **LowLevelScenarioTests**, **MediumLevelScenarioTests**, **HighLevelScenarioTests**, **SectorScenarioTests** (ESG, Carbon, compliance + classic: Fintech, Support, E‑commerce), and **WorkflowStatusTests** (process status transitions: Draft→InProgress, full lifecycle to Approved/Completed, Rejected path, stuck Draft/InProgress, EU Green Bond style).

See [Setup](setup.md) to run the sample and [Testing](testing.md) to run the tests.
