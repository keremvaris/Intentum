using MediatR;

namespace Intentum.Cqrs.Web.Features.SampleFeature.Commands;

public sealed record SampleCommand(string Name) : IRequest<SampleResult>;
public sealed record SampleResult(string Id, string Name);
