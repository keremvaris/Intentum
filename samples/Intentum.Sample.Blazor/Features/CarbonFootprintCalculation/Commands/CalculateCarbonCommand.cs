using MediatR;

namespace Intentum.Sample.Blazor.Features.CarbonFootprintCalculation.Commands;

public sealed record CalculateCarbonCommand(
    string Actor,
    string Scope,
    decimal? EstimatedTonsCo2 = null
) : IRequest<ICalculateCarbonResponse>;
