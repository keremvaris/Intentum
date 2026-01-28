namespace Intentum.Sample.Web.Api;

internal record InferIntentRequest(IReadOnlyList<IntentEventDto> Events);
internal record IntentEventDto(string Actor, string Action);
internal record InferIntentResponse(
    string Decision,
    string Confidence,
    bool RateLimitAllowed,
    int? RateLimitCurrent,
    int? RateLimitLimit,
    string HistoryId);
