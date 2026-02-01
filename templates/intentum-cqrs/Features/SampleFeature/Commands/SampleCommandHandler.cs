using MediatR;

namespace Intentum.Cqrs.Web.Features.SampleFeature.Commands;

public sealed class SampleCommandHandler : IRequestHandler<SampleCommand, SampleResult>
{
    public Task<SampleResult> Handle(SampleCommand request, CancellationToken cancellationToken)
        => Task.FromResult(new SampleResult(Guid.NewGuid().ToString("N")[..8], request.Name));
}
