using MediatR;

namespace Intentum.Sample.Blazor.Features.OrderPlacement.Commands;

public sealed record PlaceOrderCommand(
    string ProductId,
    int Quantity,
    string CustomerId
) : IRequest<PlaceOrderResult>;
