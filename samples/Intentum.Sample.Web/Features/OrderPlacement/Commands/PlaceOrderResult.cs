namespace Intentum.Sample.Web.Features.OrderPlacement.Commands;

public sealed record PlaceOrderResult(string OrderId, string ProductId, int Quantity);
