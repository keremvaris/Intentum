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
using Intentum.Sample;

// ---------------------------------------------------------------------------
// INTENTUM SAMPLE — Showcase: ESG, Carbon Accounting, Sustainability
// ---------------------------------------------------------------------------
// AI pipeline: behavior keys (e.g. user:login) → embedding provider → similarity engine → confidence + signals.
// - Mock (default): no API key; deterministic hash-based scores.
// - Real AI: set OPENAI_API_KEY (and optionally OPENAI_EMBEDDING_MODEL) to use OpenAI embeddings.
// Run: dotnet run --project samples/Intentum.Sample
// ---------------------------------------------------------------------------

IIntentEmbeddingProvider embeddingProvider;
// ReSharper disable once InconsistentNaming
var useOpenAI = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
if (useOpenAI)
{
    var options = OpenAIOptions.FromEnvironment();
    options.Validate();
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(options.BaseUrl!)
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
        i => i.Signals.Count(s => s.Description.Contains(SampleLiterals.Retry, StringComparison.OrdinalIgnoreCase)) >= 3,
        PolicyDecision.Block))
    .AddRule(new PolicyRule(
        "ComplianceRiskBlock",
        i => i.Signals.Any(s => s.Description.Contains(SampleLiterals.Compliance, StringComparison.OrdinalIgnoreCase) &&
                                i.Confidence.Level == SampleLiterals.Low),
        PolicyDecision.Block))
    .AddRule(new PolicyRule(
        "HighConfidenceAllow",
        i => i.Confidence.Level is SampleLiterals.High or SampleLiterals.Certain,
        PolicyDecision.Allow))
    .AddRule(new PolicyRule(
        "MediumConfidenceObserve",
        i => i.Confidence.Level == SampleLiterals.Medium,
        PolicyDecision.Observe))
    .AddRule(new PolicyRule(
        "LowConfidenceWarn",
        i => i.Confidence.Level == SampleLiterals.Low,
        PolicyDecision.Warn));

var localizer = new DefaultLocalizer("tr");

Console.WriteLine();
Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  INTENTUM SAMPLE — ESG, Carbon, EU Green Bond, Workflows  ║");
Console.WriteLine("║  + Classic (Payment, Support, E‑commerce) + ProcessStatus flows  ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
Console.WriteLine($"  AI: {(useOpenAI ? "OpenAI (embedding provider)" : "Mock (no API key)")} → similarity → confidence → policy");
Console.WriteLine();

// ---- LOW COMPLEXITY (ESG & Carbon) ----
PrintSection("LOW COMPLEXITY — ESG & Carbon");
RunScenario("Carbon footprint calculation", SampleLiterals.Low, SampleLiterals.Esg, space => space
    .Observe(SampleLiterals.Analyst, SampleLiterals.CalculateCarbon)
    .Observe(SampleLiterals.System, SampleLiterals.ReportGenerated));
RunScenario("ESG metric view", SampleLiterals.Low, SampleLiterals.Esg, space => space
    .Observe(SampleLiterals.User, SampleLiterals.ViewEsgMetric));
RunScenario("Compliance check", SampleLiterals.Low, SampleLiterals.ComplianceSector, space => space
    .Observe(SampleLiterals.Compliance, SampleLiterals.CheckIcma)
    .Observe(SampleLiterals.System, SampleLiterals.Validated));

// ---- MEDIUM COMPLEXITY (ESG Reporting & Verification) ----
PrintSection("MEDIUM COMPLEXITY — ESG Reporting");
RunScenario("ESG report submission with retries", SampleLiterals.Medium, SampleLiterals.Esg, space => space
    .Observe(SampleLiterals.Analyst, SampleLiterals.PrepareEsgReport)
    .Observe(SampleLiterals.Analyst, SampleLiterals.RetryValidation)
    .Observe(SampleLiterals.Analyst, SampleLiterals.RetryValidation)
    .Observe(SampleLiterals.System, SampleLiterals.ReportSubmitted));
RunScenario("Carbon verification process", SampleLiterals.Medium, SampleLiterals.Carbon, space => space
    .Observe(SampleLiterals.Verifier, SampleLiterals.VerifyCarbonData)
    .Observe(SampleLiterals.Verifier, SampleLiterals.RequestCorrection)
    .Observe(SampleLiterals.Analyst, SampleLiterals.SubmitCorrection)
    .Observe(SampleLiterals.Verifier, SampleLiterals.Approve));
RunScenario("LMA loan compliance check", SampleLiterals.Medium, SampleLiterals.ComplianceSector, space => space
    .Observe(SampleLiterals.Lender, SampleLiterals.CheckLmaCompliance)
    .Observe(SampleLiterals.System, SampleLiterals.FlagIssue)
    .Observe(SampleLiterals.Lender, SampleLiterals.Resolve)
    .Observe(SampleLiterals.System, SampleLiterals.ComplianceOk));

// ---- HIGH COMPLEXITY (Multi-actor ESG & Compliance) ----
PrintSection("HIGH COMPLEXITY — Multi-actor ESG");
RunScenario("ESG compliance audit trail", SampleLiterals.High, SampleLiterals.Esg, space => space
    .Observe(SampleLiterals.Analyst, SampleLiterals.PrepareEsgReport)
    .Observe(SampleLiterals.Compliance, SampleLiterals.ReviewEsg)
    .Observe(SampleLiterals.Compliance, SampleLiterals.FlagDiscrepancy)
    .Observe(SampleLiterals.Analyst, SampleLiterals.RetryCorrection)
    .Observe(SampleLiterals.Compliance, SampleLiterals.Approve)
    .Observe(SampleLiterals.System, SampleLiterals.PublishEsg));
RunScenario("Carbon accounting with multiple validators", SampleLiterals.High, SampleLiterals.Carbon, space => space
    .Observe(SampleLiterals.Analyst, SampleLiterals.CalculateCarbon)
    .Observe(SampleLiterals.InternalAudit, SampleLiterals.Review)
    .Observe(SampleLiterals.ExternalVerifier, SampleLiterals.Verify)
    .Observe(SampleLiterals.ExternalVerifier, SampleLiterals.RequestChanges)
    .Observe(SampleLiterals.Analyst, SampleLiterals.Update)
    .Observe(SampleLiterals.ExternalVerifier, SampleLiterals.Certify));
RunScenario("ESG risk assessment with multiple stakeholders", SampleLiterals.High, SampleLiterals.Esg, space => space
    .Observe(SampleLiterals.Analyst, SampleLiterals.AssessEsgRisk)
    .Observe(SampleLiterals.RiskCommittee, SampleLiterals.Review)
    .Observe(SampleLiterals.RiskCommittee, SampleLiterals.RequestDetails)
    .Observe(SampleLiterals.Analyst, SampleLiterals.ProvideDetails)
    .Observe(SampleLiterals.RiskCommittee, SampleLiterals.Approve)
    .Observe(SampleLiterals.Board, SampleLiterals.FinalApproval));

// ---- SECTOR: ESG & SUSTAINABILITY ----
PrintSection("SECTOR: ESG & SUSTAINABILITY");
RunScenario("ESG report happy path", SampleLiterals.Medium, SampleLiterals.Esg, space => space
    .Observe(SampleLiterals.Analyst, SampleLiterals.PrepareEsgReport)
    .Observe(SampleLiterals.Compliance, SampleLiterals.Approve)
    .Observe(SampleLiterals.System, SampleLiterals.PublishEsg));
RunScenario("ESG report with compliance issues", SampleLiterals.High, SampleLiterals.Esg, space => space
    .Observe(SampleLiterals.Analyst, SampleLiterals.PrepareEsgReport)
    .Observe(SampleLiterals.Compliance, SampleLiterals.FlagIssue)
    .Observe(SampleLiterals.Analyst, SampleLiterals.RetryCorrection)
    .Observe(SampleLiterals.Analyst, SampleLiterals.RetryCorrection)
    .Observe(SampleLiterals.Compliance, SampleLiterals.Approve));
RunScenario("Sustainability metric tracking", SampleLiterals.Medium, SampleLiterals.Esg, space => space
    .Observe(SampleLiterals.Analyst, SampleLiterals.TrackSustainability)
    .Observe(SampleLiterals.Analyst, SampleLiterals.UpdateMetric)
    .Observe(SampleLiterals.System, SampleLiterals.ValidateAction)
    .Observe(SampleLiterals.System, SampleLiterals.Record));

// ---- SECTOR: CARBON ACCOUNTING ----
PrintSection("SECTOR: CARBON ACCOUNTING");
RunScenario("Carbon calculation success", SampleLiterals.Low, SampleLiterals.Carbon, space => space
    .Observe(SampleLiterals.Analyst, SampleLiterals.CalculateCarbon)
    .Observe(SampleLiterals.System, SampleLiterals.ValidateAction)
    .Observe(SampleLiterals.System, SampleLiterals.Record));
RunScenario("Carbon verification with corrections", SampleLiterals.Medium, SampleLiterals.Carbon, space => space
    .Observe(SampleLiterals.Analyst, SampleLiterals.CalculateCarbon)
    .Observe(SampleLiterals.Verifier, SampleLiterals.Verify)
    .Observe(SampleLiterals.Verifier, SampleLiterals.RequestCorrection)
    .Observe(SampleLiterals.Analyst, SampleLiterals.Correct)
    .Observe(SampleLiterals.Verifier, SampleLiterals.Approve));
RunScenario("Carbon audit trail", SampleLiterals.High, SampleLiterals.Carbon, space => space
    .Observe(SampleLiterals.Analyst, SampleLiterals.CalculateCarbon)
    .Observe(SampleLiterals.InternalAudit, SampleLiterals.Review)
    .Observe(SampleLiterals.ExternalVerifier, SampleLiterals.Verify)
    .Observe(SampleLiterals.ExternalVerifier, SampleLiterals.Certify));

// ---- SECTOR: EU GREEN BOND ----
PrintSection("SECTOR: EU GREEN BOND");
RunScenario("EU Green Bond draft to in-progress", SampleLiterals.Low, SampleLiterals.EuGreenBond, space => space
    .Observe(SampleLiterals.Process, SampleLiterals.Draft)
    .Observe(SampleLiterals.Process, SampleLiterals.InProgress));
RunScenario("EU Green Bond under review to approved", SampleLiterals.Medium, SampleLiterals.EuGreenBond, space => space
    .Observe(SampleLiterals.Process, SampleLiterals.Draft)
    .Observe(SampleLiterals.Process, SampleLiterals.InProgress)
    .Observe(SampleLiterals.Process, SampleLiterals.UnderReview)
    .Observe(SampleLiterals.Process, SampleLiterals.ProcessApproved));
RunScenario("EU Green Bond full lifecycle to completed", SampleLiterals.High, SampleLiterals.EuGreenBond, space => space
    .Observe(SampleLiterals.Process, SampleLiterals.Draft)
    .Observe(SampleLiterals.Process, SampleLiterals.InProgress)
    .Observe(SampleLiterals.Process, SampleLiterals.UnderReview)
    .Observe(SampleLiterals.Process, SampleLiterals.ProcessApproved)
    .Observe(SampleLiterals.Process, SampleLiterals.Completed));
RunScenario("EU Green Bond rejected path", SampleLiterals.Medium, SampleLiterals.EuGreenBond, space => space
    .Observe(SampleLiterals.Process, SampleLiterals.Draft)
    .Observe(SampleLiterals.Process, SampleLiterals.InProgress)
    .Observe(SampleLiterals.Process, SampleLiterals.UnderReview)
    .Observe(SampleLiterals.Process, SampleLiterals.Rejected));

// ---- WORKFLOW PROCESS STATUS (Draft → InProgress → UnderReview → Approved/Rejected → Completed) ----
PrintSection("WORKFLOW PROCESS STATUS — Transitions");
RunScenario("ESG report: Draft → InProgress → UnderReview → Approved", SampleLiterals.Medium, SampleLiterals.Workflow, space => space
    .Observe(SampleLiterals.Process, SampleLiterals.Draft)
    .Observe(SampleLiterals.Process, SampleLiterals.InProgress)
    .Observe(SampleLiterals.Process, SampleLiterals.UnderReview)
    .Observe(SampleLiterals.Process, SampleLiterals.ProcessApproved));
RunScenario("Compliance workflow: Draft → InProgress → UnderReview → Rejected", SampleLiterals.Medium, SampleLiterals.Workflow, space => space
    .Observe(SampleLiterals.Process, SampleLiterals.Draft)
    .Observe(SampleLiterals.Process, SampleLiterals.InProgress)
    .Observe(SampleLiterals.Process, SampleLiterals.UnderReview)
    .Observe(SampleLiterals.Process, SampleLiterals.Rejected));
RunScenario("LMA loan: Draft → InProgress (stuck)", SampleLiterals.Low, SampleLiterals.Workflow, space => space
    .Observe(SampleLiterals.Process, SampleLiterals.Draft)
    .Observe(SampleLiterals.Process, SampleLiterals.InProgress));

// ---- CLASSIC EXAMPLES (Payment, Support, E‑commerce) ----
PrintSection("CLASSIC EXAMPLES — Payment, Support, E‑commerce");
RunScenario("Payment happy path", SampleLiterals.Low, SampleLiterals.Fintech, space => space
    .Observe(SampleLiterals.User, "login")
    .Observe(SampleLiterals.User, SampleLiterals.Submit));
RunScenario("Payment with retries", SampleLiterals.Medium, SampleLiterals.Fintech, space => space
    .Observe(SampleLiterals.User, "login")
    .Observe(SampleLiterals.User, SampleLiterals.Retry)
    .Observe(SampleLiterals.User, SampleLiterals.Retry)
    .Observe(SampleLiterals.User, SampleLiterals.Submit));
RunScenario("Suspicious retries (no submit)", SampleLiterals.Medium, SampleLiterals.Fintech, space => space
    .Observe(SampleLiterals.User, "login")
    .Observe(SampleLiterals.User, SampleLiterals.Retry)
    .Observe(SampleLiterals.User, SampleLiterals.Retry)
    .Observe(SampleLiterals.User, SampleLiterals.Retry));
RunScenario("Support escalation", SampleLiterals.Medium, SampleLiterals.Support, space => space
    .Observe(SampleLiterals.User, SampleLiterals.Ask)
    .Observe(SampleLiterals.User, SampleLiterals.Ask)
    .Observe(SampleLiterals.System, SampleLiterals.Escalate));
RunScenario("E‑commerce add to cart / product view", SampleLiterals.Low, SampleLiterals.ECommerce, space => space
    .Observe(SampleLiterals.User, SampleLiterals.ViewProduct)
    .Observe(SampleLiterals.User, SampleLiterals.AddToCart));
RunScenario("E‑commerce checkout success", SampleLiterals.Low, SampleLiterals.ECommerce, space => space
    .Observe(SampleLiterals.User, SampleLiterals.Cart)
    .Observe(SampleLiterals.User, SampleLiterals.Checkout)
    .Observe(SampleLiterals.User, SampleLiterals.Submit));
RunScenario("E‑commerce checkout with retries", SampleLiterals.Medium, SampleLiterals.ECommerce, space => space
    .Observe(SampleLiterals.User, SampleLiterals.Cart)
    .Observe(SampleLiterals.User, SampleLiterals.Checkout)
    .Observe(SampleLiterals.User, SampleLiterals.Retry)
    .Observe(SampleLiterals.User, SampleLiterals.Submit));
RunScenario("E‑commerce checkout with payment validation", SampleLiterals.Medium, SampleLiterals.ECommerce, space => space
    .Observe(SampleLiterals.User, SampleLiterals.Cart)
    .Observe(SampleLiterals.User, SampleLiterals.Checkout)
    .Observe(SampleLiterals.User, SampleLiterals.PaymentAttempt)
    .Observe(SampleLiterals.User, SampleLiterals.Retry)
    .Observe(SampleLiterals.System, SampleLiterals.PaymentValidate)
    .Observe(SampleLiterals.User, SampleLiterals.Submit));

Console.WriteLine("Done. See docs/audience.md for ESG, Carbon, EU Green Bond, workflow status, e‑commerce, and classic use cases.");
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
