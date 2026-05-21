namespace OutboxSaga.Orders.Api.Contracts;

public sealed record CreateOrderRequest(
    string CustomerId,
    string CustomerName,
    string CustomerEmail,
    string Description,
    decimal TotalValue
);
