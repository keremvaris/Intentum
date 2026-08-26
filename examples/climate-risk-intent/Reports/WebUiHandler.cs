using System.Net;
using System.Text;
using System.Text.Json;
using Intentum.Core.Behavior;
using Intentum.Example.ClimateRisk.Intents;
using Intentum.Example.ClimateRisk.Models;
using Intentum.Example.ClimateRisk.Policy;
using Intentum.Example.ClimateRisk.Risks;
using Intentum.Example.ClimateRisk.Scenarios;
using Intentum.Runtime.Engine;

namespace Intentum.Example.ClimateRisk.Reports;

public static class WebUiHandler
{
    public static async Task StartServer(int port = 5000)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        Console.WriteLine($"Web UI running at http://localhost:{port}/");

        while (true)
        {
            var context = await listener.GetContextAsync();
            _ = Task.Run(() => HandleRequest(context));
        }
    }

    private static async Task HandleRequest(HttpListenerContext context)
    {
        var response = context.Response;
        var path = context.Request.Url?.AbsolutePath ?? "/";

        response.ContentType = path.EndsWith(".html") || path == "/" ? "text/html" : "application/json";

        string content = path switch
        {
            "/" or "/index.html" => GetHtml(),
            "/api/scenarios" => GetScenarios(),
            "/api/risks" when context.Request.HttpMethod == "POST" => await GetRiskAssessment(context.Request),
            "/api/health" => JsonSerializer.Serialize(new { status = "healthy" }),
            _ => "{\"error\":\"not found\"}"
        };

        var buffer = Encoding.UTF8.GetBytes(content);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.Close();
    }

    private static string GetScenarios()
    {
        var scenarios = SspScenarios.All.Concat(RcpScenarios.All).Concat(WriScenarios.All);
        return JsonSerializer.Serialize(scenarios.Select(s => new { s.Id, s.Name, s.Description, type = s.Type.ToString() }));
    }

    private static async Task<string> GetRiskAssessment(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var body = await reader.ReadToEndAsync();
        var param = JsonSerializer.Deserialize<AssessmentRequest>(body);

        var scenario = SspScenarios.GetById(param?.ScenarioId ?? "SSP2-4.5") ?? SspScenarios.Ssp2_45;
        var sector = SectorProfile.GetById(param?.Sector ?? "Energy") ?? SectorProfile.Energy;
        var horizon = param?.Horizon == 2030 ? TimeHorizon.NearTerm2030 :
                      param?.Horizon == 2100 ? TimeHorizon.LongTerm2100 :
                      TimeHorizon.MediumTerm2050;

        var assessment = BuildAssessment(scenario, sector, horizon);
        return JsonSerializer.Serialize(new
        {
            assessment.Scenario.Id,
            assessment.Scenario.Name,
            Sector = sector.Name,
            Horizon = horizon.ToString(),
            PhysicalRisk = assessment.PhysicalRiskScore,
            TransitionRisk = assessment.TransitionRiskScore,
            OverallRisk = assessment.OverallRiskScore,
            PhysicalFactors = assessment.PhysicalFactors,
            TransitionFactors = assessment.TransitionFactors,
            assessment.RecommendedActions
        });
    }

    private static RiskAssessment BuildAssessment(ClimateScenario scenario, SectorProfile sector, TimeHorizon horizon)
    {
        var (physicalScore, physicalFactors) = PhysicalRiskCalculator.Calculate(scenario, sector, horizon);
        var (transitionScore, transitionFactors) = TransitionRiskCalculator.Calculate(scenario, sector, horizon);

        var space = new BehaviorSpace();
        foreach (var f in physicalFactors)
            for (var i = 0; i < Math.Ceiling(f.WeightedScore * 5); i++)
                space.Observe(new BehaviorEvent("physical", f.Name.ToLowerInvariant(), DateTimeOffset.UtcNow));

        foreach (var f in transitionFactors)
            for (var i = 0; i < Math.Ceiling(f.WeightedScore * 5); i++)
                space.Observe(new BehaviorEvent("transition", f.Name.ToLowerInvariant(), DateTimeOffset.UtcNow));

        var model = new ClimateRiskIntentModel();
        var policy = ClimateRiskPolicy.Create();

        return new RiskAssessment(
            Scenario: scenario,
            Sector: sector,
            Horizon: horizon,
            PhysicalRiskScore: physicalScore,
            TransitionRiskScore: transitionScore,
            EconomicImpactScore: (physicalScore + transitionScore) / 2.0,
            PhysicalFactors: physicalFactors,
            TransitionFactors: transitionFactors,
            RecommendedActions: GetActions(scenario, sector, physicalScore, transitionScore));
    }

    private static IReadOnlyList<string> GetActions(ClimateScenario scenario, SectorProfile sector, double physical, double transition)
    {
        var actions = new List<string>();

        if (physical > 0.6)
        {
            actions.Add($"Physical risk high for {sector.Name}: implement climate adaptation measures");
            actions.Add("Develop business continuity plans for extreme weather events");
        }

        if (transition > 0.6)
        {
            actions.Add($"Transition risk elevated: assess {sector.Name} exposure to policy and technology changes");
            actions.Add("Develop transition roadmap aligned with scenario pathway");
        }

        if (physical > 0.4 && physical <= 0.6)
            actions.Add("Monitor physical risk indicators quarterly");

        if (transition > 0.4 && transition <= 0.6)
            actions.Add("Track regulatory developments and technology cost curves");

        if (actions.Count == 0)
            actions.Add("Continue standard risk monitoring procedures");

        return actions;
    }

    private static string GetHtml() => """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Climate Risk Assessment - Intentum</title>
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background: #0f172a; color: #e2e8f0; }
                .container { max-width: 1200px; margin: 0 auto; padding: 2rem; }
                h1 { font-size: 1.5rem; margin-bottom: 1.5rem; color: #38bdf8; }
                .controls { display: flex; gap: 1rem; margin-bottom: 2rem; flex-wrap: wrap; }
                select, button { padding: 0.5rem 1rem; border-radius: 6px; border: 1px solid #334155; background: #1e293b; color: #e2e8f0; font-size: 0.875rem; }
                button { background: #2563eb; border-color: #2563eb; cursor: pointer; }
                button:hover { background: #1d4ed8; }
                .results { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; }
                .card { background: #1e293b; border-radius: 8px; padding: 1.25rem; border: 1px solid #334155; }
                .card h3 { font-size: 0.875rem; color: #94a3b8; margin-bottom: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em; }
                .score { font-size: 2rem; font-weight: 700; }
                .score.high { color: #ef4444; }
                .score.medium { color: #f59e0b; }
                .score.low { color: #22c55e; }
                .factor { display: flex; justify-content: space-between; padding: 0.35rem 0; border-bottom: 1px solid #334155; font-size: 0.875rem; }
                .actions { grid-column: 1 / -1; }
                .actions li { padding: 0.5rem 0; border-bottom: 1px solid #334155; list-style: none; }
                .actions li::before { content: "• "; color: #38bdf8; }
                .intent-badge { display: inline-block; padding: 0.25rem 0.75rem; border-radius: 9999px; font-size: 0.75rem; font-weight: 600; margin-top: 0.5rem; }
                .intent-badge.critical { background: #7f1d1d; color: #fca5a5; }
                .intent-badge.elevated { background: #78350f; color: #fcd34d; }
                .intent-badge.moderate { background: #713f12; color: #fde68a; }
                .intent-badge.low { background: #14532d; color: #86efac; }
                .full-width { grid-column: 1 / -1; }
                #loading { display: none; text-align: center; padding: 2rem; color: #94a3b8; }
            </style>
        </head>
        <body>
            <div class="container">
                <h1>Climate Risk Assessment</h1>
                <div class="controls">
                    <select id="scenario">
                        <optgroup label="SSP Scenarios">
                            <option value="SSP1-2.6">SSP1-2.6 (Sustainable)</option>
                            <option value="SSP2-4.5" selected>SSP2-4.5 (Middle Road)</option>
                            <option value="SSP3-7.0">SSP3-7.0 (Regional Rivalry)</option>
                            <option value="SSP5-8.5">SSP5-8.5 (Fossil-Fueled)</option>
                        </optgroup>
                        <optgroup label="RCP Scenarios">
                            <option value="RCP2.6">RCP2.6 (Peak & Decline)</option>
                            <option value="RCP4.5">RCP4.5 (Stabilization)</option>
                            <option value="RCP6.0">RCP6.0 (High Stabilization)</option>
                            <option value="RCP8.5">RCP8.5 (Very High)</option>
                        </optgroup>
                        <optgroup label="WRI Scenarios">
                            <option value="WRI-WATER-LOW">WRI Water Stress Low</option>
                            <option value="WRI-WATER-HIGH">WRI Water Stress High</option>
                            <option value="WRI-ENERGY-FAST">WRI Fast Energy Transition</option>
                            <option value="WRI-ENERGY-SLOW">WRI Slow Energy Transition</option>
                        </optgroup>
                    </select>
                    <select id="sector">
                        <option value="Energy" selected>Energy</option>
                        <option value="Agriculture">Agriculture</option>
                        <option value="RealEstate">Real Estate</option>
                        <option value="Finance">Finance</option>
                        <option value="Tourism">Tourism</option>
                    </select>
                    <select id="horizon">
                        <option value="2030">2030 (Near-term)</option>
                        <option value="2050" selected>2050 (Medium-term)</option>
                        <option value="2100">2100 (Long-term)</option>
                    </select>
                    <button onclick="assess()">Assess Risk</button>
                </div>
                <div id="loading">Loading...</div>
                <div id="results" class="results" style="display:none;"></div>
            </div>
            <script>
                async function assess() {
                    document.getElementById('loading').style.display = 'block';
                    document.getElementById('results').style.display = 'none';
                    const res = await fetch('/api/risks', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            scenarioId: document.getElementById('scenario').value,
                            sector: document.getElementById('sector').value,
                            horizon: parseInt(document.getElementById('horizon').value)
                        })
                    });
                    const data = await res.json();
                    render(data);
                }
                function scoreClass(s) { return s > 0.6 ? 'high' : s > 0.3 ? 'medium' : 'low'; }
                function intentClass(name) {
                    if (name.includes('Critical')) return 'critical';
                    if (name.includes('Elevated')) return 'elevated';
                    if (name.includes('Moderate')) return 'moderate';
                    return 'low';
                }
                function render(d) {
                    const html = `
                        <div class="card"><h3>Physical Risk</h3>
                            <div class="score ${scoreClass(d.physicalRisk)}">${d.physicalRisk.toFixed(2)}</div>
                            ${(d.physicalFactors||[]).map(f => `<div class="factor"><span>${f.name}</span><span>${f.weightedScore.toFixed(3)}</span></div>`).join('')}
                        </div>
                        <div class="card"><h3>Transition Risk</h3>
                            <div class="score ${scoreClass(d.transitionRisk)}">${d.transitionRisk.toFixed(2)}</div>
                            ${(d.transitionFactors||[]).map(f => `<div class="factor"><span>${f.name}</span><span>${f.weightedScore.toFixed(3)}</span></div>`).join('')}
                        </div>
                        <div class="card full-width"><h3>Recommended Actions</h3>
                            <ul class="actions">${(d.recommendedActions||[]).map(a => `<li>${a}</li>`).join('')}</ul>
                        </div>
                    `;
                    document.getElementById('results').innerHTML = html;
                    document.getElementById('results').style.display = 'grid';
                    document.getElementById('loading').style.display = 'none';
                }
            </script>
        </body>
        </html>
        """;

    private sealed class AssessmentRequest
    {
        public string? ScenarioId { get; set; }
        public string? Sector { get; set; }
        public int? Horizon { get; set; }
    }
}
