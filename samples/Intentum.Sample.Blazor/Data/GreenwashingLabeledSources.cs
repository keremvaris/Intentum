namespace Intentum.Sample.Blazor.Data;

/// <summary>
/// Etiketli greenwashing kaynakları — docs/case-studies/greenwashing-labeled-sources.csv ve
/// greenwashing-sources.md'den derlendi. ClientEarth, Guardian, ASA, Mendeley, Reuters vb.
/// </summary>
public static class GreenwashingLabeledSources
{
    private const string ActiveGreenwashing = "ActiveGreenwashing";
    private const string ClientEarth = "ClientEarth";
    private const string Guardian = "Guardian";
    private const string GenuineSustainability = "GenuineSustainability";

    public sealed record Row(string Url, string HumanLabel, string SourceName, string Notes);

    /// <summary>53 satırlık tam setin özeti; demo'da 30 kaynak gösteriliyor.</summary>
    public const int TotalCount = 53;

    public static IReadOnlyList<Row> GetAll()
    {
        return Rows;
    }

    private static readonly List<Row> Rows = new()
    {
        new("https://www.clientearth.org/projects/the-greenwashing-files/aramco/", ActiveGreenwashing, ClientEarth, "Aramco"),
        new("https://www.clientearth.org/projects/the-greenwashing-files/chevron/", ActiveGreenwashing, ClientEarth, "Chevron"),
        new("https://www.clientearth.org/projects/the-greenwashing-files/drax/", ActiveGreenwashing, ClientEarth, "Drax"),
        new("https://www.clientearth.org/projects/the-greenwashing-files/equinor/", ActiveGreenwashing, ClientEarth, "Equinor"),
        new("https://www.clientearth.org/projects/the-greenwashing-files/exxonmobil/", ActiveGreenwashing, ClientEarth, "ExxonMobil"),
        new("https://www.clientearth.org/projects/the-greenwashing-files/ineos/", ActiveGreenwashing, ClientEarth, "INEOS"),
        new("https://www.clientearth.org/projects/the-greenwashing-files/rwe/", ActiveGreenwashing, ClientEarth, "RWE"),
        new("https://www.clientearth.org/projects/the-greenwashing-files/shell/", ActiveGreenwashing, ClientEarth, "Shell"),
        new("https://www.clientearth.org/projects/the-greenwashing-files/total/", ActiveGreenwashing, ClientEarth, "Total"),
        new("https://www.theguardian.com/environment/2022/feb/02/activists-accuse-drinks-firm-innocent-greenwashing-plastics-rebellion-advertising-tv-advert", ActiveGreenwashing, Guardian, "Innocent Drinks"),
        new("https://www.bbc.com/news/business-60481080", ActiveGreenwashing, "BBC", "Innocent ads banned"),
        new("https://www.bbc.co.uk/news/business-60366054", ActiveGreenwashing, "BBC", "HSBC fossil fuel financing"),
        new("https://www.theguardian.com/business/2020/nov/03/shells-climate-poll-on-twitter-backfires-spectacularly", ActiveGreenwashing, Guardian, "Shell Twitter poll"),
        new("https://www.theguardian.com/business/2022/oct/19/watchdog-bans-hsbc-ads-green-cop26-climate-crisis", ActiveGreenwashing, Guardian, "HSBC climate ads"),
        new("https://adfreecities.org.uk/2023/06/shell-adverts-banned-for-greenwashing/", ActiveGreenwashing, "Adfree Cities", "Shell clean energy ads"),
        new("https://adfreecities.org.uk/2024/12/lloyds-bank-ads-banned-for-greenwashing/", ActiveGreenwashing, "Adfree Cities", "Lloyds LinkedIn ads"),
        new("https://www.asa.org.uk/rulings/shell-international-ltd-a22-1116032-shell-international-ltd.html", ActiveGreenwashing, "ASA", "Shell Energy 2022"),
        new("https://www.reuters.com/sustainability/boards-policy-regulation/italian-regulator-hits-shein-with-1-million-euro-greenwashing-fine-2025-08-04/", ActiveGreenwashing, "Reuters", "Shein fine"),
        new("https://www.greenpeace.fr/totalenergies-condamnee-greenwashing/", ActiveGreenwashing, "Greenpeace France", "TotalEnergies"),
        new("https://www.earthsight.org.uk/flatpackedforests-en", ActiveGreenwashing, "Earthsight", "IKEA illegal logging"),
        new("https://www.cbc.ca/news/business/keurig-fined-3-million-fine-1.6307150", "SelectiveDisclosure", "CBC", "Keurig recycling claims"),
        new("https://www.asa.org.uk/rulings/lavazza-coffee-uk-ltd.html", "SelectiveDisclosure", "ASA", "Lavazza compostability"),
        new("https://www.asa.org.uk/rulings/nike-retail-bv-a25-1309100-nike-retail-bv.html", "StrategicObfuscation", "ASA", "Nike sustainability claims"),
        new("https://news.sky.com/story/ryanair-adverts-banned-for-making-misleading-co2-emissions-claims-11926471", "StrategicObfuscation", "Sky", "Ryanair low emissions"),
        new("https://www.theguardian.com/business/2020/feb/14/delta-carbon-neutral-airline-plan", "StrategicObfuscation", Guardian, "Delta carbon neutral"),
        new("https://www.asa.org.uk/rulings/marlow-foods-ltd-g20-1061634-marlow-foods-ltd.html", "UnintentionalMisrepresentation", "ASA", "Quorn carbon footprint"),
        new("https://www.apple.com/environment/pdf/Apple_Environmental_Progress_Report_2025.pdf", GenuineSustainability, "Apple", "Environmental Progress Report"),
        new("https://cdn-dynmedia-1.microsoft.com/is/content/microsoftcorp/microsoft/msc/documents/presentations/CSR/2025-Microsoft-Environmental-Sustainability-Report-PDF.pdf", GenuineSustainability, "Microsoft", "Environmental Sustainability Report"),
        new("https://sustainability.aboutamazon.com/2024-amazon-sustainability-report.pdf", GenuineSustainability, "Amazon", "Sustainability Report"),
        new("https://www.gates.com/content/dam/gates/home/about-us/sustainability/sustainability-report-library/gates-2023-sustainability-report.pdf", GenuineSustainability, "Gates Foundation", "Sustainability Report"),
        new("https://www.stellantis.com/content/dam/stellantis-corporate/sustainability/csr-disclosure/stellantis/2023/Stellantis-2023-CSR-Report.pdf", GenuineSustainability, "Stellantis", "CSR Report"),
        new("https://www.gri.org/", GenuineSustainability, "GRI", "GRI Standards"),
        new("https://data.mendeley.com/datasets/vv5695ywmn/1", "dataset", "Mendeley", "ESG and Greenwashing dataset (CC BY 4.0)"),
        new("https://thesustainableagency.com/blog/greenwashing-examples/", "greenwashing", "The Sustainable Agency", "Article listing 21 cases"),
    };
}
