# Audience, project types & example use cases (EN)

This page describes **which types of projects** use Intentum, **who the users** are (developer profiles), and **example test cases** at low, medium, and high complexity — for both **AI-driven** and **non-AI (rule-based)** usage. It also gives **sector-based** examples (ESG, Carbon Accounting, Sukuk & Islamic Finance, Compliance) so you can map Intentum to your domain.

For the core flow (Observe → Infer → Decide), see [index](index.md) and [API Reference](api.md). For runnable scenarios, see the [sample](https://github.com/keremvaris/Intentum/tree/master/samples/Intentum.Sample) and [Scenarios](scenarios.md).

---

## Project types: where Intentum fits

| Project type | Typical use | Why Intentum |
|--------------|-------------|--------------|
| **ESG & Sustainability** | ESG reporting, compliance, metric tracking, risk assessment | Observe ESG events (report preparation, compliance review, verification); infer intent; policy decides Allow / Block / Observe. Non-deterministic (multiple stakeholders, compliance checks). |
| **Carbon Accounting** | Carbon footprint calculation, verification, audit trails | Observe carbon calculation and verification events; infer intent; policy decides allow / flag / block based on compliance. |
| **Sukuk & Islamic Finance** | Sukuk issuance, sharia review, ICMA compliance, regulatory approval | Observe issuance flow (sharia review, regulatory checks, ICMA compliance); infer intent; policy decides allow / block / observe. |
| **Compliance & audit** | ICMA, LMA compliance checks, audit trails, risk flags | Observe compliance events; infer risk level; policy decides allow / flag / block. |
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
| 3 | **Sukuk issuance inquiry** | `investor:inquire_sukuk` → `system:provide_details` | Allow High; Observe Medium | Allow or Observe | Both |
| 4 | **ICMA compliance check** | `compliance:check_icma` → `system:validated` | First rule: Low → Warn | Warn | Normal (rule-based) |

### Medium complexity (3–4 examples)

| # | Name | Behavior (Observe) | Policy idea | Expected | AI / normal |
|---|------|--------------------|------------|----------|-------------|
| 1 | **ESG report submission with retries** | prepare_esg_report → retry_validation → retry_validation → report_submitted | Allow High; Observe Medium; Block only if retry ≥ 3 | Allow or Observe | Both |
| 2 | **Carbon verification process** | verify_carbon_data → request_correction → submit_correction → approve | Allow on approval; Observe on corrections | Allow or Observe | Both |
| 3 | **Sukuk issuance with approvals** | initiate_sukuk → sharia:review → regulator:approve → issue_sukuk | Allow High; Observe Medium; Block on compliance risk | Allow or Observe | Both |
| 4 | **LMA loan compliance check** | check_lma_compliance → flag_issue → resolve → compliance_ok | Block or Warn on compliance issues | Block or Warn | Normal (rule-based) |

### High complexity (3–4 examples)

| # | Name | Behavior (Observe) | Policy idea | Expected | AI / normal |
|---|------|--------------------|------------|----------|-------------|
| 1 | **ESG compliance audit trail** | prepare_esg_report, compliance:review_esg, flag_discrepancy, retry_correction, approve, publish_esg | Block when compliance risk + excessive retries; else Allow High | Block or Allow | Both |
| 2 | **Carbon accounting with multiple validators** | calculate_carbon, internal_audit:review, external_verifier:verify, request_changes, update, certify | Infer intent from embeddings; policy by confidence and signal counts | Allow / Observe / Warn | AI |
| 3 | **Sukuk issuance with sharia and regulatory review** | initiate_sukuk, sharia:review, request_amendment, amend, regulator:review, approve, issue_sukuk | Block on compliance risk; Allow on High confidence | Block or Allow | Both |
| 4 | **ESG risk assessment with multiple stakeholders** | assess_esg_risk, risk_committee:review, request_details, provide_details, approve, board:final_approval | Block on compliance risk; Observe on Medium; Allow on High | Block or Observe or Allow | Both |

---

## Sector-based example usages

| Sector | Example flow | Observe | Infer | Decide |
|--------|--------------|--------|-------|--------|
| **ESG** | ESG report happy path | prepare_esg_report, compliance:approve, publish_esg | Intent + confidence | Allow / Observe |
| **ESG** | ESG report with compliance issues | prepare_esg_report, flag_issue, retry_correction×2, approve | Intent + signals | Block if excessive retries; else Allow |
| **Carbon** | Carbon calculation success | calculate_carbon, validate, record | Intent + confidence | Allow |
| **Carbon** | Carbon verification with corrections | calculate_carbon, verify, request_correction, correct, approve | Intent | Allow or Observe |
| **Sukuk** | Sukuk issuance inquiry | inquire_sukuk, provide_details | Intent | Allow |
| **Sukuk** | Sukuk with sharia review | initiate_sukuk, sharia:review, approve, issue_sukuk | Intent + confidence | Allow or Observe |
| **Sukuk** | Sukuk with ICMA compliance | initiate_sukuk, sharia:review, icma:check_compliance, request_adjustment, adjust, approve, issue_sukuk | Intent + signals | Block on compliance risk; else Allow |
| **Compliance** | ICMA compliance check | check_icma, validated | Risk intent | Warn or Allow |
| **EU Green Bond** | Draft → InProgress → UnderReview → Approved → Completed | process:Draft, InProgress, UnderReview, Approved, Completed | Intent + signals | Allow on Completed |
| **EU Green Bond** | Rejected path | process:Draft, InProgress, UnderReview, Rejected | Intent + signals | Block on Rejected |
| **Classic (Fintech)** | Payment happy path / with retries | login, retry, submit | Intent + confidence | Allow / Observe; Block if excessive retries |
| **Classic (Support)** | Escalation | user:ask, user:ask, system:escalate | Intent | Warn or Allow |
| **Classic (E‑commerce)** | Checkout success / with retries | cart, checkout, retry, submit | Intent | Allow / Observe; Block if excessive retries |

---

## Workflow process status (Draft → InProgress → UnderReview → Approved / Rejected → Completed)

Many complex workflows (ICMA, LMA, Sukuk, EU Green Bond, ESG reporting) use a **process status** lifecycle. You Observe status transitions; the model infers intent; policy decides Allow / Block / Observe.

| Status / transition | Observe | Policy idea | Expected |
|---------------------|--------|-------------|----------|
| **Draft → InProgress** | `process:Draft`, `process:InProgress` | Allow or Observe (work in progress) | Allow or Observe |
| **Draft → InProgress → UnderReview → Approved** | Draft, InProgress, UnderReview, Approved | Allow when Approved signal present | Allow |
| **Draft → InProgress → UnderReview → Approved → Completed** | Full lifecycle | Allow when Completed | Allow |
| **Draft → InProgress → UnderReview → Rejected** | Draft, InProgress, UnderReview, Rejected | Block when Rejected | Block |
| **Stuck in Draft / InProgress** | Draft, InProgress only | Observe (no Approved/Rejected/Completed) | Observe or Allow |

The sample and **WorkflowStatusTests** include these transitions for ESG report, Sukuk issuance, ICMA compliance, LMA loan, and EU Green Bond style workflows.

---

## How the sample and tests map to this

- **Sample project** runs: ESG/Carbon/Sukuk/EU Green Bond scenarios; **workflow process status** (Draft, InProgress, UnderReview, Approved, Rejected, Completed) for EU Green Bond, ESG, Sukuk, ICMA, LMA; and **classic** examples (payment happy path, payment with retries, suspicious retries, support escalation, e‑commerce checkout).
- **Test project** has: **LowLevelScenarioTests**, **MediumLevelScenarioTests**, **HighLevelScenarioTests**, **SectorScenarioTests** (ESG, Carbon, Sukuk, ICMA + classic: Fintech, Support, E‑commerce), and **WorkflowStatusTests** (process status transitions: Draft→InProgress, full lifecycle to Approved/Completed, Rejected path, stuck Draft/InProgress, EU Green Bond style).

See [Setup](setup.md) to run the sample and [Testing](testing.md) to run the tests.
