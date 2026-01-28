using Intentum.AI.Embeddings;
using Intentum.AI.Mock;
using Intentum.AI.Models;
using Intentum.AI.Similarity;
using Intentum.AI.OpenAI;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Runtime;
using Intentum.Runtime.Localization;
using Intentum.Runtime.Policy;

// ---------------------------------------------------------------------------
// INTENTUM SAMPLE — Showcase: ESG, Carbon Accounting, Sukuk, Sustainability
// ---------------------------------------------------------------------------
// AI pipeline: behavior keys (e.g. user:login) → embedding provider → similarity engine → confidence + signals.
// - Mock (default): no API key; deterministic hash-based scores.
// - Real AI: set OPENAI_API_KEY (and optionally OPENAI_EMBEDDING_MODEL) to use OpenAI embeddings.
// Run: dotnet run --project samples/Intentum.Sample
// ---------------------------------------------------------------------------

IIntentEmbeddingProvider embeddingProvider;
var useOpenAI = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
if (useOpenAI)
{
    var options = OpenAIOptions.FromEnvironment();
    options.Validate();
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(options.BaseUrl ?? "https://api.openai.com/v1/")
    };
    httpClient.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
    embeddingProvider = new OpenAIEmbeddingProvider(options, httpClient);
}
else
{
    embeddingProvider = new MockEmbeddingProvider();
}

var similarityEngine = new SimpleAverageSimilarityEngine();
var intentModel = new LlmIntentModel(embeddingProvider, similarityEngine);

var policy = new IntentPolicy()
    .AddRule(new PolicyRule(
        "ExcessiveRetryBlock",
        i => i.Signals.Count(s => s.Description.Contains("retry", StringComparison.OrdinalIgnoreCase)) >= 3,
        PolicyDecision.Block))
    .AddRule(new PolicyRule(
        "ComplianceRiskBlock",
        i => i.Signals.Any(s => s.Description.Contains("compliance", StringComparison.OrdinalIgnoreCase) && 
                                i.Confidence.Level == "Low"),
        PolicyDecision.Block))
    .AddRule(new PolicyRule(
        "HighConfidenceAllow",
        i => i.Confidence.Level is "High" or "Certain",
        PolicyDecision.Allow))
    .AddRule(new PolicyRule(
        "MediumConfidenceObserve",
        i => i.Confidence.Level == "Medium",
        PolicyDecision.Observe))
    .AddRule(new PolicyRule(
        "LowConfidenceWarn",
        i => i.Confidence.Level == "Low",
        PolicyDecision.Warn));

var localizer = new DefaultLocalizer("tr");

Console.WriteLine();
Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  INTENTUM SAMPLE — ESG, Carbon, Sukuk, EU Green Bond, Workflows  ║");
Console.WriteLine("║  + Classic (Payment, Support, E‑commerce) + ProcessStatus flows  ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
Console.WriteLine($"  AI: {(useOpenAI ? "OpenAI (embedding provider)" : "Mock (no API key)")} → similarity → confidence → policy");
Console.WriteLine();

// ---- LOW COMPLEXITY (ESG & Carbon) ----
PrintSection("LOW COMPLEXITY — ESG & Carbon");
RunScenario("Carbon footprint calculation", "Low", "ESG", space => space
    .Observe("analyst", "calculate_carbon")
    .Observe("system", "report_generated"));
RunScenario("ESG metric view", "Low", "ESG", space => space
    .Observe("user", "view_esg_metric"));
RunScenario("Sukuk issuance inquiry", "Low", "Finance", space => space
    .Observe("investor", "inquire_sukuk")
    .Observe("system", "provide_details"));
RunScenario("ICMA compliance check", "Low", "Compliance", space => space
    .Observe("compliance", "check_icma")
    .Observe("system", "validated"));

// ---- MEDIUM COMPLEXITY (ESG Reporting & Verification) ----
PrintSection("MEDIUM COMPLEXITY — ESG Reporting");
RunScenario("ESG report submission with retries", "Medium", "ESG", space => space
    .Observe("analyst", "prepare_esg_report")
    .Observe("analyst", "retry_validation")
    .Observe("analyst", "retry_validation")
    .Observe("system", "report_submitted"));
RunScenario("Carbon verification process", "Medium", "Carbon", space => space
    .Observe("verifier", "verify_carbon_data")
    .Observe("verifier", "request_correction")
    .Observe("analyst", "submit_correction")
    .Observe("verifier", "approve"));
RunScenario("Sukuk issuance with approvals", "Medium", "Finance", space => space
    .Observe("issuer", "initiate_sukuk")
    .Observe("sharia", "review")
    .Observe("regulator", "approve")
    .Observe("system", "issue_sukuk"));
RunScenario("LMA loan compliance check", "Medium", "Compliance", space => space
    .Observe("lender", "check_lma_compliance")
    .Observe("system", "flag_issue")
    .Observe("lender", "resolve")
    .Observe("system", "compliance_ok"));

// ---- HIGH COMPLEXITY (Multi-actor ESG & Compliance) ----
PrintSection("HIGH COMPLEXITY — Multi-actor ESG");
RunScenario("ESG compliance audit trail", "High", "ESG", space => space
    .Observe("analyst", "prepare_esg_report")
    .Observe("compliance", "review_esg")
    .Observe("compliance", "flag_discrepancy")
    .Observe("analyst", "retry_correction")
    .Observe("compliance", "approve")
    .Observe("system", "publish_esg"));
RunScenario("Carbon accounting with multiple validators", "High", "Carbon", space => space
    .Observe("analyst", "calculate_carbon")
    .Observe("internal_audit", "review")
    .Observe("external_verifier", "verify")
    .Observe("external_verifier", "request_changes")
    .Observe("analyst", "update")
    .Observe("external_verifier", "certify"));
RunScenario("Sukuk issuance with sharia and regulatory review", "High", "Finance", space => space
    .Observe("issuer", "initiate_sukuk")
    .Observe("sharia", "review")
    .Observe("sharia", "request_amendment")
    .Observe("issuer", "amend")
    .Observe("regulator", "review")
    .Observe("regulator", "approve")
    .Observe("system", "issue_sukuk"));
RunScenario("ESG risk assessment with multiple stakeholders", "High", "ESG", space => space
    .Observe("analyst", "assess_esg_risk")
    .Observe("risk_committee", "review")
    .Observe("risk_committee", "request_details")
    .Observe("analyst", "provide_details")
    .Observe("risk_committee", "approve")
    .Observe("board", "final_approval"));

// ---- SECTOR: ESG & SUSTAINABILITY ----
PrintSection("SECTOR: ESG & SUSTAINABILITY");
RunScenario("ESG report happy path", "Medium", "ESG", space => space
    .Observe("analyst", "prepare_esg_report")
    .Observe("compliance", "approve")
    .Observe("system", "publish_esg"));
RunScenario("ESG report with compliance issues", "High", "ESG", space => space
    .Observe("analyst", "prepare_esg_report")
    .Observe("compliance", "flag_issue")
    .Observe("analyst", "retry_correction")
    .Observe("analyst", "retry_correction")
    .Observe("compliance", "approve"));
RunScenario("Sustainability metric tracking", "Medium", "ESG", space => space
    .Observe("analyst", "track_sustainability")
    .Observe("analyst", "update_metric")
    .Observe("system", "validate")
    .Observe("system", "record"));

// ---- SECTOR: CARBON ACCOUNTING ----
PrintSection("SECTOR: CARBON ACCOUNTING");
RunScenario("Carbon calculation success", "Low", "Carbon", space => space
    .Observe("analyst", "calculate_carbon")
    .Observe("system", "validate")
    .Observe("system", "record"));
RunScenario("Carbon verification with corrections", "Medium", "Carbon", space => space
    .Observe("analyst", "calculate_carbon")
    .Observe("verifier", "verify")
    .Observe("verifier", "request_correction")
    .Observe("analyst", "correct")
    .Observe("verifier", "approve"));
RunScenario("Carbon audit trail", "High", "Carbon", space => space
    .Observe("analyst", "calculate_carbon")
    .Observe("internal_audit", "review")
    .Observe("external_verifier", "verify")
    .Observe("external_verifier", "certify"));

// ---- SECTOR: SUKUK & ISLAMIC FINANCE ----
PrintSection("SECTOR: SUKUK & ISLAMIC FINANCE");
RunScenario("Sukuk inquiry flow", "Low", "Finance", space => space
    .Observe("investor", "inquire_sukuk")
    .Observe("system", "provide_details"));
RunScenario("Sukuk issuance with sharia review", "Medium", "Finance", space => space
    .Observe("issuer", "initiate_sukuk")
    .Observe("sharia", "review")
    .Observe("sharia", "approve")
    .Observe("system", "issue_sukuk"));
RunScenario("Sukuk compliance with ICMA standards", "High", "Finance", space => space
    .Observe("issuer", "initiate_sukuk")
    .Observe("sharia", "review")
    .Observe("icma", "check_compliance")
    .Observe("icma", "request_adjustment")
    .Observe("issuer", "adjust")
    .Observe("icma", "approve")
    .Observe("system", "issue_sukuk"));

// ---- SECTOR: EU GREEN BOND ----
PrintSection("SECTOR: EU GREEN BOND");
RunScenario("EU Green Bond draft to in-progress", "Low", "EU Green Bond", space => space
    .Observe("process", "Draft")
    .Observe("process", "InProgress"));
RunScenario("EU Green Bond under review to approved", "Medium", "EU Green Bond", space => space
    .Observe("process", "Draft")
    .Observe("process", "InProgress")
    .Observe("process", "UnderReview")
    .Observe("process", "Approved"));
RunScenario("EU Green Bond full lifecycle to completed", "High", "EU Green Bond", space => space
    .Observe("process", "Draft")
    .Observe("process", "InProgress")
    .Observe("process", "UnderReview")
    .Observe("process", "Approved")
    .Observe("process", "Completed"));
RunScenario("EU Green Bond rejected path", "Medium", "EU Green Bond", space => space
    .Observe("process", "Draft")
    .Observe("process", "InProgress")
    .Observe("process", "UnderReview")
    .Observe("process", "Rejected"));

// ---- WORKFLOW PROCESS STATUS (Draft → InProgress → UnderReview → Approved/Rejected → Completed) ----
PrintSection("WORKFLOW PROCESS STATUS — Transitions");
RunScenario("ESG report: Draft → InProgress → UnderReview → Approved", "Medium", "Workflow", space => space
    .Observe("process", "Draft")
    .Observe("process", "InProgress")
    .Observe("process", "UnderReview")
    .Observe("process", "Approved"));
RunScenario("Sukuk issuance: Draft → InProgress → UnderReview → Approved → Completed", "High", "Workflow", space => space
    .Observe("process", "Draft")
    .Observe("process", "InProgress")
    .Observe("process", "UnderReview")
    .Observe("process", "Approved")
    .Observe("process", "Completed"));
RunScenario("ICMA compliance: Draft → InProgress → UnderReview → Rejected", "Medium", "Workflow", space => space
    .Observe("process", "Draft")
    .Observe("process", "InProgress")
    .Observe("process", "UnderReview")
    .Observe("process", "Rejected"));
RunScenario("LMA loan: Draft → InProgress (stuck)", "Low", "Workflow", space => space
    .Observe("process", "Draft")
    .Observe("process", "InProgress"));

// ---- CLASSIC EXAMPLES (Payment, Support, E‑commerce) ----
PrintSection("CLASSIC EXAMPLES — Payment, Support, E‑commerce");
RunScenario("Payment happy path", "Low", "Fintech", space => space
    .Observe("user", "login")
    .Observe("user", "submit"));
RunScenario("Payment with retries", "Medium", "Fintech", space => space
    .Observe("user", "login")
    .Observe("user", "retry")
    .Observe("user", "retry")
    .Observe("user", "submit"));
RunScenario("Suspicious retries (no submit)", "Medium", "Fintech", space => space
    .Observe("user", "login")
    .Observe("user", "retry")
    .Observe("user", "retry")
    .Observe("user", "retry"));
RunScenario("Support escalation", "Medium", "Support", space => space
    .Observe("user", "ask")
    .Observe("user", "ask")
    .Observe("system", "escalate"));
RunScenario("E‑commerce checkout success", "Low", "E‑commerce", space => space
    .Observe("user", "cart")
    .Observe("user", "checkout")
    .Observe("user", "submit"));
RunScenario("E‑commerce checkout with retries", "Medium", "E‑commerce", space => space
    .Observe("user", "cart")
    .Observe("user", "checkout")
    .Observe("user", "retry")
    .Observe("user", "submit"));

Console.WriteLine("Done. See docs/audience.md for ESG, Carbon, Sukuk, EU Green Bond, workflow status, and classic use cases.");
Console.WriteLine();

void PrintSection(string title)
{
    Console.WriteLine($"─── {title} ───");
}

void RunScenario(string name, string level, string? sector, Action<BehaviorSpace> build)
{
    var space = new BehaviorSpace();
    build(space);

    var intent = intentModel.Infer(space);
    var decision = intent.Decide(policy);
    var decisionTr = decision.ToLocalizedString(localizer);
    var vector = space.ToVector();

    Console.WriteLine($"  [{level}] {(sector != null ? $"[{sector}] " : "")}{name}");
    Console.WriteLine($"      Events: {space.Events.Count}  |  Confidence: {intent.Confidence.Level} ({intent.Confidence.Score:0.00})  |  Decision: {decision} / {decisionTr}");
    Console.WriteLine($"      Vector: {string.Join(", ", vector.Dimensions.Select(d => $"{d.Key}={d.Value}"))}");
    if (intent.Signals.Count > 0)
        Console.WriteLine($"      Signals: {string.Join("; ", intent.Signals.Take(5).Select(s => $"{s.Description}({s.Weight:0.00})"))}");
    Console.WriteLine();
}
