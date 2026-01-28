using MediatR;

namespace Intentum.Sample.Web.Features.CarbonFootprintCalculation.Queries;

public sealed record GetCarbonReportQuery(string ReportId) : IRequest<CarbonReportDto?>;
