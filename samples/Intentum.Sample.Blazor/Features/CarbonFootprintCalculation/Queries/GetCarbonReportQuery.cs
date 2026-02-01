using MediatR;

namespace Intentum.Sample.Blazor.Features.CarbonFootprintCalculation.Queries;

public sealed record GetCarbonReportQuery(string ReportId) : IRequest<CarbonReportDto?>;
