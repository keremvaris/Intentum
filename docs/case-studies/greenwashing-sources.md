# Greenwashing case study — public data sources

This document lists **publicly available sources** used or recommended for the greenwashing labeled dataset. Each source has a URL and a human label (or "dataset"). Full text does not need to live in the repo; **URL + label** (and optional extracted signal counts) are enough. Attribution and license are noted where required.

---

## 1. Mendeley Data — ESG and Greenwashing (CC BY 4.0)

| Field | Value |
|-------|--------|
| **URL** | https://data.mendeley.com/datasets/vv5695ywmn/1 |
| **DOI** | 10.17632/vv5695ywmn.1 |
| **License** | CC BY 4.0 |
| **Description** | Dataset used in research on ESG and greenwashing; multiple labeled items. |
| **Download** | On the page, click **"Download All"** to get the full dataset. No API key required; manual download only. |
| **Local path** | Unpacked dataset is in `docs/case-studies/downloaded/DataGreenwash/`. See [DataGreenwash/README.md](downloaded/DataGreenwash/README.md) for file layout. |

---

## 2. ClientEarth — The Greenwashing Files (greenwashing)

NGO analysis of fossil-fuel company advertising. Each profile is a **greenwashing** example (misleading green marketing).

| URL | Label |
|-----|--------|
| https://www.clientearth.org/projects/the-greenwashing-files/ | (index) |
| https://www.clientearth.org/projects/the-greenwashing-files/aramco/ | ActiveGreenwashing |
| https://www.clientearth.org/projects/the-greenwashing-files/chevron/ | ActiveGreenwashing |
| https://www.clientearth.org/projects/the-greenwashing-files/drax/ | ActiveGreenwashing |
| https://www.clientearth.org/projects/the-greenwashing-files/equinor/ | ActiveGreenwashing |
| https://www.clientearth.org/projects/the-greenwashing-files/exxonmobil/ | ActiveGreenwashing |
| https://www.clientearth.org/projects/the-greenwashing-files/ineos/ | ActiveGreenwashing |
| https://www.clientearth.org/projects/the-greenwashing-files/rwe/ | ActiveGreenwashing |
| https://www.clientearth.org/projects/the-greenwashing-files/shell/ | ActiveGreenwashing |
| https://www.clientearth.org/projects/the-greenwashing-files/total/ | ActiveGreenwashing |

*Source: ClientEarth, with DeSmog. No license specified; use for reference and attribution.*

---

## 3. The Sustainable Agency — 20+ greenwashing examples (greenwashing)

Article listing recent greenwashing cases with links to rulings and news.

| URL | Label |
|-----|--------|
| https://thesustainableagency.com/blog/greenwashing-examples | (index; 21 cases) |

Individual case sources (rulings, news) are in [greenwashing-labeled-sources.csv](greenwashing-labeled-sources.csv).

---

## 4. Genuine sustainability reports (GenuineSustainability)

Examples of sustainability reports often cited for transparency and GRI-aligned disclosure. Use as **GenuineSustainability** or similar when building a balanced set.

| URL | Label |
|-----|--------|
| https://www.apple.com/environment/pdf/Apple_Environmental_Progress_Report_2025.pdf | GenuineSustainability |
| https://cdn-dynmedia-1.microsoft.com/is/content/microsoftcorp/microsoft/msc/documents/presentations/CSR/2025-Microsoft-Environmental-Sustainability-Report-PDF.pdf | GenuineSustainability |
| https://sustainability.aboutamazon.com/2024-amazon-sustainability-report.pdf | GenuineSustainability |
| https://www.gates.com/content/dam/gates/home/about-us/sustainability/sustainability-report-library/gates-2023-sustainability-report.pdf | GenuineSustainability |
| https://www.stellantis.com/content/dam/stellantis-corporate/sustainability/csr-disclosure/stellantis/2023/Stellantis-2023-CSR-Report.pdf | GenuineSustainability |

*Check each site for current report URL; links may change.*

---

## How to use

1. **Extend the labeled set:** Add rows to `greenwashing-labeled-sources.csv` (url, human_label, source_name, notes).
2. **Mendeley dataset:** If not already present, visit the Mendeley URL above and click "Download All"; unpack into `docs/case-studies/downloaded/DataGreenwash/`. Map their labels to our taxonomy (GenuineSustainability, UnintentionalMisrepresentation, SelectiveDisclosure, StrategicObfuscation, ActiveGreenwashing).
3. **Download public HTML (optional):** Run `./scripts/download-greenwashing-sources.sh` from the repo root to fetch ClientEarth profile pages and the Sustainable Agency article into `docs/case-studies/downloaded/` for local analysis.
4. **Transformation rule:** For each document, extract signal counts (vague claims, unsubstantiated comparisons, metrics without proof, baseline manipulation, nature imagery without data) and build a `BehaviorSpace` as in [greenwashing-metrics.md](greenwashing-metrics.md). Then run `GreenwashingCaseStudyTests` or your evaluation pipeline and update the metrics page.
