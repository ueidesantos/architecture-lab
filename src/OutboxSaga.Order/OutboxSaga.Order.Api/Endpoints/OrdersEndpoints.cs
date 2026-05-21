using OutboxSaga.Orders.Api.Contracts;
using OutboxSaga.Orders.Application.Abstractions.UseCases;
using OutboxSaga.Orders.Application.Features.Orders.Create;

namespace OutboxSaga.Orders.Api.Endpoints;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (
            CreateOrderRequest req,
            ICommandHandler<CreateOrderCommand, CreateOrderResult> handler,
            CancellationToken ct) =>
        {
            var cmd = new CreateOrderCommand(
                req.CustomerId,
                req.CustomerName,
                req.CustomerEmail,
                req.Description,
                req.TotalValue);

            var result = await handler.HandleAsync(cmd, ct);

            return Results.Created(
                $"/orders/{result.OrderId}",
                new CreateOrderResponse(result.OrderId));
        });

        return app;
    }
}
