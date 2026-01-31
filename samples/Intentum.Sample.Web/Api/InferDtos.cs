namespace Intentum.Sample.Web.Api;

internal record InferIntentRequest(IReadOnlyList<IntentEventDto> Events);
internal record IntentEventDto(string Actor, string Action);

internal record PlaygroundCompareRequest(IReadOnlyList<IntentEventDto> Events, IReadOnlyList<string>? Providers = null);
internal record PlaygroundCompareResult(string Provider, string IntentName, string ConfidenceLevel, double ConfidenceScore, string Decision);
internal record PlaygroundCompareResponse(IReadOnlyList<PlaygroundCompareResult> Results);
internal record InferIntentResponse(
    string Decision,
    string Confidence,
    bool RateLimitAllowed,
    int? RateLimitCurrent,
    int? RateLimitLimit,
    string HistoryId);

internal record GreenwashingAnalyzeRequest(string? Report, string? SourceType, string? Language, string? ImageBase64);

public record GreenwashingScope3Supplier(string Name, bool Verified);
public record GreenwashingScope3Summary(int TotalSuppliers, int VerifiedCount, IReadOnlyList<GreenwashingScope3Supplier> Details);

internal record GreenwashingSourceMetadata(
    string SourceType,
    string? Language,
    bool Scope3Verified,
    GreenwashingScope3Summary? Scope3Summary,
    string? BlockchainRef,
    DateTimeOffset AnalyzedAt);

public record GreenwashingVisualResult(double GreenScore, string Label);

internal record GreenwashingAnalyzeResponse(
    string IntentName,
    string Confidence,
    double ConfidenceScore,
    string Decision,
    IReadOnlyList<string> SignalDescriptions,
    IReadOnlyList<string> SuggestedActions,
    string? BlockchainRef,
    GreenwashingSourceMetadata? SourceMetadata,
    GreenwashingVisualResult? VisualResult);

public record GreenwashingRecentItem(
    string Id,
    string ReportPreview,
    string IntentName,
    string Decision,
    string? SourceType,
    string? Language,
    DateTimeOffset AnalyzedAt);
