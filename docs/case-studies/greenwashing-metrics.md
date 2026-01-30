# Greenwashing detection case study — metrics

This document summarizes the **greenwashing intent** case study: transformation rule, data source, and numeric metrics (accuracy, macro F1). Goal: answer *"How well does Intentum perform in at least one domain?"* with a reproducible, documented result.

## Case study completion status (50–100 labeled public data)

| Component | Status |
|-----------|--------|
| **Labeled dataset (URL + human label)** | ✅ **greenwashing-labeled-sources.csv** — 53 rows (GenuineSustainability, ActiveGreenwashing, SelectiveDisclosure, StrategicObfuscation, UnintentionalMisrepresentation). |
| **Transformation rule (document → BehaviorSpace)** | ✅ Documented and implemented: **SustainabilityReporter.AnalyzeReport(text)** extracts counts for `language:claim.vague`, `language:comparison.unsubstantiated`, `data:metrics.without.proof`, `data:baseline.manipulation`. (Imagery from image only.) |
| **Model (BehaviorSpace → Intent)** | ✅ **GreenwashingIntentModel.Infer** — intent name + confidence; taxonomy aligned with labels. |
| **Metric computation (accuracy / macro F1)** | ✅ **GreenwashingCaseStudyTests** — accuracy and macro F1 on labeled examples. |
| **Numeric summary in repo** | ✅ This page; values from last test run. |
| **Document text for CSV URLs** | ⚠️ **Partial:** CSV has URLs only. Run `./scripts/download-greenwashing-sources.sh` to fetch ClientEarth + Sustainable Agency HTML into `docs/case-studies/downloaded/`. No automatic fetch for PDFs or other news URLs. |
| **Evaluation on public CSV rows** | ✅ **GreenwashingCaseStudyTests.GreenwashingCaseStudy_OnDownloadedHtml_ComputesAccuracyAndF1** — when downloaded HTML exists, reads CSV, maps ClientEarth URLs to local files, extracts text, runs AnalyzeReport → Infer, compares to human_label, reports accuracy/F1. Run download script first for ~10 ClientEarth rows. |
| **Evaluation on Mendeley Excel** | ✅ **GreenwashingCaseStudyTests.GreenwashingCaseStudy_OnMendeleyExcel_ComputesAccuracyAndF1** — when DataGreenwash *greenwash*.xlsx exists, reads rows (ENTITY + columns as text), runs AnalyzeReport → Infer, compares to human label (default ActiveGreenwashing), reports accuracy/F1. Cap 500 rows per run. |

**Conclusion:** Everything needed for the case study is in place. For **synthetic** labeled data (19 examples): run `GreenwashingCaseStudyTests.GreenwashingCaseStudy_ComputesAccuracyAndF1`. For **public data** (subset with local HTML): run `./scripts/download-greenwashing-sources.sh`, then `GreenwashingCaseStudyTests.GreenwashingCaseStudy_OnDownloadedHtml_ComputesAccuracyAndF1`; extend CSV and download more sources to approach 50–100 evaluated rows.

## Transformation rule (document → BehaviorSpace)

- **Actor:action** format: each dimension is `actor:action`. The greenwashing model expects these signal dimensions:
  - `language:claim.vague`
  - `language:comparison.unsubstantiated`
  - `data:metrics.without.proof`
  - `data:baseline.manipulation`
  - `imagery:nature.without.data`
- **From a document (PDF, web article, report):** extract counts per signal (e.g. how many vague claims, unsubstantiated comparisons, metrics without proof, baseline manipulation, nature imagery without data). Then build a `BehaviorSpace` by calling `Observe(actor, action)` once per occurrence (e.g. 3 vague claims → `Observe("language", "claim.vague")` three times).
- **Data source:** You do not need to create the data yourself. Public sources are valid: company sustainability reports (PDF), news/analysis articles, academic or NGO reports (greenwashing vs genuine examples). Important: each example must have a **human label** (e.g. GenuineSustainability, UnintentionalMisrepresentation, SelectiveDisclosure, StrategicObfuscation, ActiveGreenwashing). Full text does not need to live in the repo — a **URL + label list** or **extracted signal counts** is enough; if licensing/attribution is required, keep a source list under `docs/case-studies/`.

## Labeled dataset (this run)

- **Type:** Synthetic labeled examples for reproducibility (19 examples). Each row: dimension counts (`actor:action` → count) and expected intent name from the model taxonomy.
- **Intent names (GreenwashingIntentModel):** `GenuineSustainability`, `UnintentionalMisrepresentation`, `SelectiveDisclosure`, `StrategicObfuscation`, `ActiveGreenwashing`.
- **Reproduce:** Run the test `GreenwashingCaseStudyTests.GreenwashingCaseStudy_ComputesAccuracyAndF1`; it builds `BehaviorSpace`s from the labeled set, runs `GreenwashingIntentModel.Infer`, and compares predicted intent name to human label.

## Numeric summary

### Synthetic labeled set (19 examples)

| Metric       | Value  |
| ------------ | ------ |
| **Accuracy** | 0.63   |
| **Macro F1** | 0.63   |
| **N**        | 19     |

*(Values from last run of `GreenwashingCaseStudyTests.GreenwashingCaseStudy_ComputesAccuracyAndF1`; re-run the test to regenerate.)*

### Public data (downloaded HTML, ClientEarth subset)

| Metric       | Value  |
| ------------ | ------ |
| **Accuracy** | 0.00   |
| **Macro F1** | 0.00   |
| **N**        | 9 (ClientEarth company profiles with local HTML) |

*(Run: `./scripts/download-greenwashing-sources.sh` then `dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter GreenwashingCaseStudy_OnDownloadedHtml_ComputesAccuracyAndF1`. All 9 rows are human-labeled ActiveGreenwashing; model predictions on stripped HTML differ — pattern-based signal extraction may need tuning for NGO article content.)*

### Public data (Mendeley DataGreenwash Excel)

| Metric       | Value  |
| ------------ | ------ |
| **Accuracy** | 0.00   |
| **Macro F1** | 0.00   |
| **N**        | 500 (capped; *greenwash*.xlsx rows with ENTITY + columns as text) |

*(Run: unpack Mendeley dataset to `docs/case-studies/downloaded/DataGreenwash/`, then `dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter GreenwashingCaseStudy_OnMendeleyExcel_ComputesAccuracyAndF1`. Excel files are tabular (ENTITY + scores), not report text; all rows default to ActiveGreenwashing. Results indicative; for report-style text use CSV+HTML or synthetic set.)*

## Public data sources (URL + label)

- **[greenwashing-sources.md](greenwashing-sources.md)** — Curated list: Mendeley dataset (CC BY 4.0; download manually), ClientEarth Greenwashing Files, The Sustainable Agency article, genuine sustainability report URLs.
- **[greenwashing-labeled-sources.csv](greenwashing-labeled-sources.csv)** — 50+ rows: `url`, `human_label`, `source_name`, `notes`. Use to extend the labeled set or map external sources to our taxonomy.
- **Download public HTML (optional):** From repo root run `./scripts/download-greenwashing-sources.sh` to fetch ClientEarth profile pages and the Sustainable Agency article into `docs/case-studies/downloaded/` for local analysis.

## How to reproduce / extend

- **Synthetic (19 examples):** `dotnet test tests/Intentum.Tests/Intentum.Tests.csproj --filter GreenwashingCaseStudy_ComputesAccuracyAndF1`. Update the "Synthetic labeled set" table above with the printed accuracy and macro F1.
- **Public data (downloaded HTML):** Run `./scripts/download-greenwashing-sources.sh`, then `dotnet test ... --filter GreenwashingCaseStudy_OnDownloadedHtml_ComputesAccuracyAndF1`. Update the "Public data (downloaded HTML)" table with the printed values.
- **Public data (Mendeley Excel):** Unpack Mendeley dataset to `docs/case-studies/downloaded/DataGreenwash/`, then `dotnet test ... --filter GreenwashingCaseStudy_OnMendeleyExcel_ComputesAccuracyAndF1`. Update the "Public data (Mendeley DataGreenwash Excel)" table with the printed values.
- **Extend the labeled set:** Add rows to [greenwashing-labeled-sources.csv](greenwashing-labeled-sources.csv); if you add more download targets to the script or provide local text/counts, re-run the public-data test and update this page with the new accuracy/F1.
