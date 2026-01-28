namespace Intentum.Sample.Web.Features.CarbonFootprintCalculation.Queries;

public sealed record CarbonReportDto(
    string ReportId,
    string Title,
    string ConfidenceLevel,
    DateTime GeneratedAt
);
