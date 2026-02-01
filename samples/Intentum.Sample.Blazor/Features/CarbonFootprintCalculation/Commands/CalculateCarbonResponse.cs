namespace Intentum.Sample.Blazor.Features.CarbonFootprintCalculation.Commands;

public interface ICalculateCarbonResponse;

public sealed record CalculateCarbonOk(Guid ReportId, string Decision, string ConfidenceLevel) : ICalculateCarbonResponse;

public sealed record CalculateCarbonError(string Error) : ICalculateCarbonResponse;
