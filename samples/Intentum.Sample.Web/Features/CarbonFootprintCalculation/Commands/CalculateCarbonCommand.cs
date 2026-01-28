using MediatR;

namespace Intentum.Sample.Web.Features.CarbonFootprintCalculation.Commands;

public sealed record CalculateCarbonCommand(
    string Actor,
    string Scope,
    decimal? EstimatedTonsCo2 = null
) : IRequest<CalculateCarbonResponse>;
