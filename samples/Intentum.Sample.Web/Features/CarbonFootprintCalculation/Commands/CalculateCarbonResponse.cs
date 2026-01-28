namespace Intentum.Sample.Web.Features.CarbonFootprintCalculation.Commands;

public abstract record CalculateCarbonResponse;

public sealed record CalculateCarbonOk(Guid ReportId, string Decision, string ConfidenceLevel) : CalculateCarbonResponse;

public sealed record CalculateCarbonError(string Error) : CalculateCarbonResponse;
