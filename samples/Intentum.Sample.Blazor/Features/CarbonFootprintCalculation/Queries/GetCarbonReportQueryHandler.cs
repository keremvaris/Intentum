using MediatR;

namespace Intentum.Sample.Blazor.Features.CarbonFootprintCalculation.Queries;

public sealed class GetCarbonReportQueryHandler : IRequestHandler<GetCarbonReportQuery, CarbonReportDto?>
{
    public Task<CarbonReportDto?> Handle(GetCarbonReportQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ReportId) || request.ReportId.Length < 10)
            return Task.FromResult<CarbonReportDto?>(null);

        var dto = new CarbonReportDto(
            request.ReportId,
            "Sample Report",
            "Medium",
            DateTime.UtcNow.AddDays(-1)
        );
        return Task.FromResult<CarbonReportDto?>(dto);
    }
}
