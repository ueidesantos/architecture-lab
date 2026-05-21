namespace OutboxSaga.Orders.Application.Features.Orders.Create;

public sealed record CreateOrderCommand(
    string CustomerId,
    string CustomerName,
    string CustomerEmail,
    string Description,
    decimal TotalValue
);
