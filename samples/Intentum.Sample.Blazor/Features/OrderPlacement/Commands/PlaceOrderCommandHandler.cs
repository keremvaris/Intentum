using MediatR;

namespace Intentum.Sample.Blazor.Features.OrderPlacement.Commands;

public sealed class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, PlaceOrderResult>
{
    public Task<PlaceOrderResult> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = Guid.NewGuid().ToString("N")[..8];
        return Task.FromResult(new PlaceOrderResult(orderId, request.ProductId, request.Quantity));
    }
}
