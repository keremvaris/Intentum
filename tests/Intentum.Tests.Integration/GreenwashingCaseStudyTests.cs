using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Intentum.Core;
using Intentum.Core.Behavior;
using Intentum.Sample.Blazor.Features.GreenwashingDetection;
using Xunit.Abstractions;

namespace Intentum.Tests.Integration;

/// <summary>
/// Greenwashing case study: labeled behavior spaces, transformation rule, and accuracy/F1.
/// Data: synthetic labeled examples (dimension counts → human label). Reproducible; can be extended with public URLs.
/// Excluded in CI (Category=Integration): some tests require downloaded data (Mendeley Excel, HTML) — run locally after ./scripts/download-greenwashing-sources.sh.
/// </summary>
[Trait("Category", "Integration")]
public partial class GreenwashingCaseStudyTests
{
    private readonly ITestOutputHelper _output;

    public GreenwashingCaseStudyTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Dimension key format is "actor:action". GreenwashingIntentModel expects e.g. language:claim.vague, data:metrics.without.proof.
    /// </summary>
    private static BehaviorSpace ToBehaviorSpace(IReadOnlyDictionary<string, int> dimensionCounts)
    {
        var space = new BehaviorSpace();
        foreach (var (dimKey, count) in dimensionCounts)
        {
            var colon = dimKey.IndexOf(':');
            var actor = colon >= 0 ? dimKey[..colon] : "unknown";
            var action = colon >= 0 ? dimKey[(colon + 1)..] : dimKey;
            for (var i = 0; i < count; i++)
                space.Observe(actor, action);
        }
        return space;
    }

    /// <summary>
    /// Labeled examples: dimension counts (actor:action) and expected intent name from GreenwashingIntentModel taxonomy.
    /// Synthetic data for reproducibility; in practice these can come from public reports (URL + extracted counts + human label).
    /// </summary>
    private static IEnumerable<(IReadOnlyDictionary<string, int> dimensions, string expectedIntentName)> GetLabeledExamples()
    {
        yield return (new Dictionary<string, int>(), "GenuineSustainability");
        yield return (new Dictionary<string, int> { ["language:claim.vague"] = 1 }, "GenuineSustainability");
        yield return (new Dictionary<string, int> { ["language:claim.vague"] = 2, ["data:metrics.without.proof"] = 1 }, "UnintentionalMisrepresentation");
        yield return (new Dictionary<string, int> { ["data:metrics.without.proof"] = 2, ["language:comparison.unsubstantiated"] = 1 }, "SelectiveDisclosure");
        yield return (new Dictionary<string, int> { ["data:baseline.manipulation"] = 1, ["data:metrics.without.proof"] = 2 }, "SelectiveDisclosure");
        yield return (new Dictionary<string, int> { ["data:baseline.manipulation"] = 2, ["imagery:nature.without.data"] = 2 }, "StrategicObfuscation");
        yield return (new Dictionary<string, int> { ["language:comparison.unsubstantiated"] = 3, ["data:metrics.without.proof"] = 2 }, "StrategicObfuscation");
        yield return (new Dictionary<string, int> { ["data:baseline.manipulation"] = 3, ["data:metrics.without.proof"] = 2, ["language:claim.vague"] = 2 }, "ActiveGreenwashing");
        yield return (new Dictionary<string, int> { ["data:baseline.manipulation"] = 4, ["imagery:nature.without.data"] = 3 }, "ActiveGreenwashing");
        yield return (new Dictionary<string, int> { ["language:claim.vague"] = 1, ["language:comparison.unsubstantiated"] = 1 }, "GenuineSustainability");
        yield return (new Dictionary<string, int> { ["imagery:nature.without.data"] = 1 }, "GenuineSustainability");
        yield return (new Dictionary<string, int> { ["data:metrics.without.proof"] = 1 }, "GenuineSustainability");
        yield return (new Dictionary<string, int> { ["data:metrics.without.proof"] = 3, ["language:claim.vague"] = 2 }, "SelectiveDisclosure");
        yield return (new Dictionary<string, int> { ["data:baseline.manipulation"] = 1, ["language:comparison.unsubstantiated"] = 2 }, "UnintentionalMisrepresentation");
        yield return (new Dictionary<string, int> { ["data:baseline.manipulation"] = 2, ["data:metrics.without.proof"] = 2, ["language:comparison.unsubstantiated"] = 1 }, "StrategicObfuscation");
        yield return (new Dictionary<string, int> { ["data:baseline.manipulation"] = 3, ["data:metrics.without.proof"] = 3 }, "ActiveGreenwashing");
        yield return (new Dictionary<string, int> { ["language:claim.vague"] = 3, ["data:metrics.without.proof"] = 1 }, "UnintentionalMisrepresentation");
        yield return (new Dictionary<string, int> { ["imagery:nature.without.data"] = 4, ["data:baseline.manipulation"] = 1 }, "StrategicObfuscation");
        yield return (new Dictionary<string, int> { ["data:baseline.manipulation"] = 5, ["data:metrics.without.proof"] = 2, ["imagery:nature.without.data"] = 2 }, "ActiveGreenwashing");
    }

    [Fact]
    public void GreenwashingCaseStudy_ComputesAccuracyAndF1()
    {
        var model = new GreenwashingIntentModel();
        var examples = GetLabeledExamples().ToList();
        var correct = 0;
        var predictedByClass = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var actualByClass = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var truePositivesByClass = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var (dimensions, expectedName) in examples)
        {
            var space = ToBehaviorSpace(dimensions);
            var intent = model.Infer(space);
            var predicted = intent.Name;
            if (string.Equals(predicted, expectedName, StringComparison.OrdinalIgnoreCase))
                correct++;
            predictedByClass[predicted] = predictedByClass.GetValueOrDefault(predicted) + 1;
            actualByClass[expectedName] = actualByClass.GetValueOrDefault(expectedName) + 1;
            if (string.Equals(predicted, expectedName, StringComparison.OrdinalIgnoreCase))
                truePositivesByClass[predicted] = truePositivesByClass.GetValueOrDefault(predicted) + 1;
        }

        var accuracy = (double)correct / examples.Count;
        var classes = actualByClass.Keys.Union(predictedByClass.Keys).Distinct().ToList();
        var precisions = new List<double>();
        var recalls = new List<double>();
        foreach (var c in classes)
        {
            var tp = truePositivesByClass.GetValueOrDefault(c);
            var pred = predictedByClass.GetValueOrDefault(c);
            var act = actualByClass.GetValueOrDefault(c);
            precisions.Add(pred > 0 ? (double)tp / pred : 0);
            recalls.Add(act > 0 ? (double)tp / act : 0);
        }
        var macroPrecision = precisions.Count > 0 ? precisions.Average() : 0;
        var macroRecall = recalls.Count > 0 ? recalls.Average() : 0;
        var macroF1 = (macroPrecision + macroRecall) > 0
            ? 2 * macroPrecision * macroRecall / (macroPrecision + macroRecall)
            : 0;

        _output.WriteLine($"Accuracy: {accuracy:F2} ({correct}/{examples.Count})");
        _output.WriteLine($"Macro F1: {macroF1:F2}");

        Assert.True(accuracy >= 0.60, $"Case study accuracy {accuracy:F2} should be >= 0.60");
        Assert.True(macroF1 >= 0.55, $"Case study macro F1 {macroF1:F2} should be >= 0.55");
    }

    /// <summary>
    /// When downloaded HTML exists (run ./scripts/download-greenwashing-sources.sh), evaluates GreenwashingIntentModel
    /// on CSV-labeled ClientEarth rows: URL → local file, extract text → BehaviorSpace → Infer → compare to human_label.
    /// Reports accuracy and macro F1; update docs/case-studies/greenwashing-metrics.md with the result.
    /// </summary>
    [Fact]
    public void GreenwashingCaseStudy_OnDownloadedHtml_ComputesAccuracyAndF1()
    {
        var (csvPath, downloadedDir) = ResolveCaseStudyPaths();
        Assert.True(csvPath != null && downloadedDir != null && File.Exists(csvPath) && Directory.Exists(downloadedDir),
            "Downloaded HTML data missing. Run from repo root and run ./scripts/download-greenwashing-sources.sh to populate docs/case-studies/downloaded/. To exclude: --filter \"FullyQualifiedName!=Intentum.Tests.Integration.GreenwashingCaseStudyTests.GreenwashingCaseStudy_OnDownloadedHtml_ComputesAccuracyAndF1\".");

        var rows = ParseLabeledCsv(csvPath);
        var model = new GreenwashingIntentModel();
        var examples = new List<(string expectedLabel, string predictedLabel)>();

        foreach (var (url, humanLabel) in rows)
        {
            var localFile = MapClientEarthUrlToLocalFile(url, downloadedDir);
            if (localFile == null || !File.Exists(localFile))
                continue;

            var html = File.ReadAllText(localFile);
            var text = StripHtml(html);
            var space = SustainabilityReporter.AnalyzeReport(text, "en");
            var intent = model.Infer(space);
            var expected = NormalizeHumanLabel(humanLabel);
            examples.Add((expected, intent.Name));
        }

        Assert.True(examples.Count > 0,
            "No ClientEarth HTML files found in downloaded/ (run ./scripts/download-greenwashing-sources.sh).");

        var correct = examples.Count(e => string.Equals(e.expectedLabel, e.predictedLabel, StringComparison.OrdinalIgnoreCase));
        var accuracy = (double)correct / examples.Count;
        var predictedByClass = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var actualByClass = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var truePositivesByClass = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var (expectedName, predicted) in examples)
        {
            predictedByClass[predicted] = predictedByClass.GetValueOrDefault(predicted) + 1;
            actualByClass[expectedName] = actualByClass.GetValueOrDefault(expectedName) + 1;
            if (string.Equals(predicted, expectedName, StringComparison.OrdinalIgnoreCase))
                truePositivesByClass[predicted] = truePositivesByClass.GetValueOrDefault(predicted) + 1;
        }
        var classes = actualByClass.Keys.Union(predictedByClass.Keys).Distinct().ToList();
        var precisions = classes.Select(c =>
        {
            var tp = truePositivesByClass.GetValueOrDefault(c);
            var pred = predictedByClass.GetValueOrDefault(c);
            return pred > 0 ? (double)tp / pred : 0;
        }).ToList();
        var recalls = classes.Select(c =>
        {
            var tp = truePositivesByClass.GetValueOrDefault(c);
            var act = actualByClass.GetValueOrDefault(c);
            return act > 0 ? (double)tp / act : 0;
        }).ToList();
        var macroPrecision = precisions.Count > 0 ? precisions.Average() : 0;
        var macroRecall = recalls.Count > 0 ? recalls.Average() : 0;
        var macroF1 = (macroPrecision + macroRecall) > 0
            ? 2 * macroPrecision * macroRecall / (macroPrecision + macroRecall)
            : 0;

        _output.WriteLine($"Public data (downloaded HTML): N={examples.Count}");
        _output.WriteLine($"Accuracy: {accuracy:F2} ({correct}/{examples.Count})");
        _output.WriteLine($"Macro F1: {macroF1:F2}");
        _output.WriteLine("Update docs/case-studies/greenwashing-metrics.md with these values when using public data.");

        Assert.True(examples.Count >= 1, "At least one row should be evaluated when downloaded HTML exists");
    }

    private static (string? csvPath, string? downloadedDir) ResolveCaseStudyPaths()
    {
        var dir = Directory.GetCurrentDirectory();
        for (var i = 0; i < 10; i++)
        {
            var csv = Path.Combine(dir, "docs", "case-studies", "greenwashing-labeled-sources.csv");
            if (File.Exists(csv))
                return (csv, Path.Combine(dir, "docs", "case-studies", "downloaded"));
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return (null, null);
    }

    private static List<(string url, string humanLabel)> ParseLabeledCsv(string csvPath)
    {
        var rows = new List<(string, string)>();
        var lines = File.ReadAllLines(csvPath);
        for (var i = 0; i < lines.Length; i++)
        {
            if (i == 0 && lines[i].StartsWith("url,human_label", StringComparison.OrdinalIgnoreCase))
                continue;
            var parts = lines[i].Split(',');
            if (parts.Length >= 2)
                rows.Add((parts[0].Trim().Trim('"'), parts[1].Trim().Trim('"')));
        }
        return rows;
    }

    private static string? MapClientEarthUrlToLocalFile(string url, string downloadedDir)
    {
        const string prefix = "https://www.clientearth.org/projects/the-greenwashing-files/";
        if (!url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return null;
        var path = url[prefix.Length..].TrimEnd('/');
        var name = path.Split('/')[0];
        if (string.IsNullOrEmpty(name)) return null;
        return Path.Combine(downloadedDir, $"clientearth-{name}.html");
    }

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    private static string StripHtml(string html)
    {
        var text = HtmlTagRegex().Replace(html, " ");
        return WhitespaceRegex().Replace(text, " ").Trim();
    }

    private static string NormalizeHumanLabel(string label)
    {
        return label.Trim().ToLowerInvariant() switch
        {
            "greenwashing" => "ActiveGreenwashing",
            "dataset" => "ActiveGreenwashing",
            _ => label.Trim()
        };
    }

    /// <summary>
    /// Resolves path to docs/case-studies/downloaded/DataGreenwash (Mendeley dataset). Walk up from cwd.
    /// </summary>
    private static string? ResolveDataGreenwashPath()
    {
        var dir = Directory.GetCurrentDirectory();
        for (var i = 0; i < 10; i++)
        {
            var dataGreenwash = Path.Combine(dir, "docs", "case-studies", "downloaded", "DataGreenwash");
            if (Directory.Exists(dataGreenwash))
                return dataGreenwash;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return null;
    }

    /// <summary>
    /// Reads labeled text + expected intent from Mendeley DataGreenwash *greenwash*.xlsx files.
    /// Detects text column (Abstract, Text, Content, Title) and label column (Label, Category, Class) by header name.
    /// If no label column, defaults to ActiveGreenwashing for greenwash-named files.
    /// </summary>
    private static List<(string Text, string ExpectedLabel)> ReadLabeledRowsFromMendeleyExcel(string dataGreenwashDir, ITestOutputHelper? output = null)
    {
        var results = new List<(string, string)>();
        var greenwashFiles = Directory.GetFiles(dataGreenwashDir, "*greenwash*.xlsx")
            .Where(f => !Path.GetFileName(f).Contains("greenwashb")) // avoid duplicate 2012greenwashb
            .OrderBy(Path.GetFileName)
            .ToList();
        if (greenwashFiles.Count == 0)
            return results;

        foreach (var filePath in greenwashFiles)
        {
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var sheet = workbook.Worksheets.First();
                var usedRange = sheet.RangeUsed();
                if (usedRange == null) continue;
                var rows = usedRange.Rows().ToList();
                if (rows.Count < 2) continue;

                var headerRow = rows[0];
                var lastCol = usedRange.LastColumn().ColumnNumber();
                int textCol = -1, labelCol = -1;
                var textNames = new[] { "abstract", "text", "content", "title", "description", "summary" };
                var labelNames = new[] { "label", "category", "class", "greenwash", "type", "intent" };
                for (var c = 1; c <= lastCol; c++)
                {
                    var h = headerRow.Cell(c).GetString().Trim().ToLowerInvariant();
                    if (textCol < 0 && textNames.Any(t => h.Contains(t)))
                        textCol = c;
                    if (labelCol < 0 && labelNames.Any(l => h.Contains(l)))
                        labelCol = c;
                }
                if (textCol < 0)
                    textCol = 1;
                var defaultLabel = "ActiveGreenwashing";
                if (output != null && rows.Count > 1)
                    output.WriteLine($"[DataGreenwash] {Path.GetFileName(filePath)}: {rows.Count - 1} rows, col1={headerRow.Cell(1).GetString()}, textCol={textCol}, labelCol={labelCol}");

                for (var r = 2; r <= rows.Count; r++)
                {
                    var row = rows[r - 1];
                    var text = GetRowText(row, textCol, lastCol);
                    if (string.IsNullOrWhiteSpace(text) || text.Length < 10)
                        continue;
                    var label = labelCol >= 0 ? row.Cell(labelCol).GetString().Trim() : defaultLabel;
                    if (string.IsNullOrWhiteSpace(label))
                        label = defaultLabel;
                    var mapped = MapMendeleyLabelToTaxonomy(label);
                    results.Add((text, mapped));
                }
            }
            catch (Exception ex)
            {
                output?.WriteLine($"Skip {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        return results;
    }

    /// <summary>
    /// Gets text from a row: prefer textCol; if too short, use first column with length >= 15; else concatenate all non-empty cells.
    /// </summary>
    private static string GetRowText(IXLRangeRow row, int textCol, int lastCol)
    {
        var primary = (row.Cell(textCol).GetString() ?? row.Cell(textCol).GetFormattedString() ?? "").Trim();
        if (primary.Length >= 15)
            return primary;
        for (var c = 1; c <= lastCol; c++)
        {
            var s = (row.Cell(c).GetString() ?? row.Cell(c).GetFormattedString() ?? "").Trim();
            if (s.Length >= 15)
                return s;
        }
        var parts = new List<string>();
        for (var c = 1; c <= lastCol; c++)
        {
            var s = (row.Cell(c).GetString() ?? row.Cell(c).GetFormattedString() ?? "").Trim();
            if (s.Length > 0)
                parts.Add(s);
        }
        return string.Join(" ", parts);
    }

    private static string MapMendeleyLabelToTaxonomy(string label)
    {
        var lower = label.Trim().ToLowerInvariant();
        if (lower.Contains("greenwash") || lower == "1" || lower == "yes" || lower == "positive")
            return "ActiveGreenwashing";
        if (lower.Contains("genuine") || lower.Contains("sustainab") || lower == "0" || lower == "no" || lower == "negative")
            return "GenuineSustainability";
        if (lower.Contains("selective") || lower.Contains("disclos"))
            return "SelectiveDisclosure";
        if (lower.Contains("strategic") || lower.Contains("obfuscat"))
            return "StrategicObfuscation";
        if (lower.Contains("unintentional") || lower.Contains("misrep"))
            return "UnintentionalMisrepresentation";
        return "ActiveGreenwashing";
    }

    /// <summary>
    /// When DataGreenwash (Mendeley) Excel files exist, evaluates GreenwashingIntentModel on rows with text + label.
    /// Reports accuracy and macro F1; update docs/case-studies/greenwashing-metrics.md with the result.
    /// </summary>
    [Fact]
    public void GreenwashingCaseStudy_OnMendeleyExcel_ComputesAccuracyAndF1()
    {
        var dataGreenwashDir = ResolveDataGreenwashPath();
        Assert.True(dataGreenwashDir != null && Directory.Exists(dataGreenwashDir),
            "DataGreenwash not found. Unpack Mendeley dataset to docs/case-studies/downloaded/DataGreenwash/. To exclude: --filter \"FullyQualifiedName!=Intentum.Tests.Integration.GreenwashingCaseStudyTests.GreenwashingCaseStudy_OnMendeleyExcel_ComputesAccuracyAndF1\".");

        var rows = ReadLabeledRowsFromMendeleyExcel(dataGreenwashDir, _output);
        Assert.True(rows.Count > 0,
            "No labeled rows with text found in DataGreenwash *greenwash*.xlsx files.");
        const int maxRows = 500;
        if (rows.Count > maxRows)
        {
            rows = rows.Take(maxRows).ToList();
            _output.WriteLine($"Capping to first {maxRows} rows for evaluation.");
        }

        var model = new GreenwashingIntentModel();
        var examples = new List<(string expectedLabel, string predictedLabel)>();
        foreach (var (text, expectedLabel) in rows)
        {
            var space = SustainabilityReporter.AnalyzeReport(text, "en");
            var intent = model.Infer(space);
            examples.Add((expectedLabel, intent.Name));
        }

        var correct = examples.Count(e => string.Equals(e.expectedLabel, e.predictedLabel, StringComparison.OrdinalIgnoreCase));
        var accuracy = (double)correct / examples.Count;
        var predictedByClass = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var actualByClass = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var truePositivesByClass = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var (expectedName, predicted) in examples)
        {
            predictedByClass[predicted] = predictedByClass.GetValueOrDefault(predicted) + 1;
            actualByClass[expectedName] = actualByClass.GetValueOrDefault(expectedName) + 1;
            if (string.Equals(predicted, expectedName, StringComparison.OrdinalIgnoreCase))
                truePositivesByClass[predicted] = truePositivesByClass.GetValueOrDefault(predicted) + 1;
        }
        var classes = actualByClass.Keys.Union(predictedByClass.Keys).Distinct().ToList();
        var macroPrecision = classes.Count > 0
            ? classes.Average(c => predictedByClass.GetValueOrDefault(c) > 0 ? (double)truePositivesByClass.GetValueOrDefault(c) / predictedByClass.GetValueOrDefault(c) : 0)
            : 0;
        var macroRecall = classes.Count > 0
            ? classes.Average(c => actualByClass.GetValueOrDefault(c) > 0 ? (double)truePositivesByClass.GetValueOrDefault(c) / actualByClass.GetValueOrDefault(c) : 0)
            : 0;
        var macroF1 = (macroPrecision + macroRecall) > 0 ? 2 * macroPrecision * macroRecall / (macroPrecision + macroRecall) : 0;

        _output.WriteLine($"Public data (Mendeley DataGreenwash): N={examples.Count}");
        _output.WriteLine($"Accuracy: {accuracy:F2} ({correct}/{examples.Count})");
        _output.WriteLine($"Macro F1: {macroF1:F2}");
        _output.WriteLine("Update docs/case-studies/greenwashing-metrics.md with these values when using Mendeley Excel.");

        Assert.True(examples.Count >= 1, "At least one row should be evaluated when DataGreenwash Excel exists");
    }
}
