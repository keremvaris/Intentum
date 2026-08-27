using System.Globalization;
using System.Text;

namespace Intentum.Sample.Blazor.Components.Services.ClimateData;

/// <summary>
/// Şirket profili CSV (Excel uyumlu) içe aktarıcı.
/// Format: her şirket "SHIRKET,..." satırıyla başlar; ardından "Kategori,..." başlıklı
/// finansal kalem satırları gelir. Birden fazla şirket desteklenir.
/// </summary>
public static class CompanyProfileImporter
{
    public static ImportResult Parse(string csvText)
    {
        if (string.IsNullOrWhiteSpace(csvText))
            return ImportResult.Fail("Dosya boş.");

        var lines = CsvParser.ParseLines(csvText);
        if (lines.Count == 0)
            return ImportResult.Fail("Dosyada veri satırı yok.");

        var companies = new List<CompanyProfile>();
        CompanyProfile? current = null;
        var hasCompany = false;

        foreach (var line in lines)
        {
            if (line.Count == 0) continue;

            var first = line[0].Trim();
            if (first.Equals("SHIRKET", StringComparison.OrdinalIgnoreCase))
            {
                current = TryParseCompany(line);
                if (current == null)
                    return ImportResult.Fail($"Şirket satırı geçersiz: {string.Join(',', line)}");
                companies.Add(current);
                hasCompany = true;
                continue;
            }

            if (first.Equals("Kategori", StringComparison.OrdinalIgnoreCase))
                continue; // başlık satırı

            // Finansal kalem satırı — geçerli bir şirket bloğu içinde olmalı.
            if (current == null)
                return ImportResult.Fail("Finansal kalem satırı bir 'SHIRKET' satırından önce gelemez.");

            TryParseLineItem(current, line);
        }

        if (!hasCompany)
            return ImportResult.Fail("Dosyada 'SHIRKET' satırı bulunamadı.");

        return ImportResult.Success(companies);
    }

    private static CompanyProfile? TryParseCompany(IReadOnlyList<string> cells)
    {
        // SHIRKET,Ad,Sektor,Lokasyon,Enlem,Boylam
        if (cells.Count < 6) return null;

        if (!double.TryParse(cells[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat))
            return null;
        if (!double.TryParse(cells[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
            return null;

        return new CompanyProfile
        {
            Name = cells[1].Trim(),
            Sector = cells[2].Trim(),
            LocationName = cells[3].Trim(),
            Latitude = lat,
            Longitude = lng
        };
    }

    private static void TryParseLineItem(CompanyProfile profile, IReadOnlyList<string> cells)
    {
        // Kategori,KategoriAdi,KalemAdi,Value,PhysSens,TransSens,Sensitivity,AdaptiveCapacity,Signals
        if (cells.Count < 4) return;

        if (!Enum.TryParse<FinancialCategoryType>(cells[0].Trim(), ignoreCase: true, out var type))
            return;
        if (!double.TryParse(cells[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return;

        var physSens = ParseDouble(cells, 4) ?? 0;
        var transSens = ParseDouble(cells, 5) ?? 0;
        var sensitivity = ParseDouble(cells, 6) ?? 0;
        var adaptiveCapacity = ParseDouble(cells, 7) ?? 1.0;
        var signals = cells.Count > 8
            ? cells[8].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : [];

        if (profile.Categories.LastOrDefault() is { } last && last.Type == type)
        {
            last.LineItems.Add(new FinancialLineItem
            {
                Name = cells[2].Trim(),
                Value = value,
                PhysicalSensitivity = physSens,
                TransitionSensitivity = transSens,
                Sensitivity = sensitivity,
                AdaptiveCapacity = adaptiveCapacity,
                MappedRiskSignals = signals
            });
        }
        else
        {
            var category = new FinancialCategory
            {
                Type = type,
                Name = cells[1].Trim(),
                LineItems =
                [
                    new FinancialLineItem
                    {
                        Name = cells[2].Trim(),
                        Value = value,
                        PhysicalSensitivity = physSens,
                        TransitionSensitivity = transSens,
                        Sensitivity = sensitivity,
                        AdaptiveCapacity = adaptiveCapacity,
                        MappedRiskSignals = signals
                    }
                ]
            };
            profile.Categories.Add(category);
        }
    }

    private static double? ParseDouble(IReadOnlyList<string> cells, int index)
        => index < cells.Count && double.TryParse(cells[index], NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : null;
}

public sealed class ImportResult
{
    public bool IsSuccess { get; init; }
    public List<CompanyProfile>? Profiles { get; init; }
    public string? Error { get; init; }

    public CompanyProfile? Profile => Profiles?.FirstOrDefault();

    public static ImportResult Success(List<CompanyProfile> profiles) => new()
    {
        IsSuccess = true,
        Profiles = profiles
    };

    public static ImportResult Fail(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}

/// <summary>Basit CSV satır ayrıştırıcı — tırnak desteği, yorum satırları (#) ve başlık satırları.</summary>
internal static class CsvParser
{
    public static List<List<string>> ParseLines(string csvText)
    {
        var rows = new List<List<string>>();
        var text = csvText.Replace("\r\n", "\n").Replace('\r', '\n');

        foreach (var rawLine in text.Split('\n'))
        {
            var trimmed = rawLine.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;

            rows.Add(ParseLine(rawLine));
        }

        return rows;
    }

    public static List<string> ParseLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                result.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        result.Add(current.ToString().Trim());
        return result;
    }
}
